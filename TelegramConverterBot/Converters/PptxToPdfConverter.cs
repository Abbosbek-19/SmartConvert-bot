using Microsoft.Extensions.Logging;
using TelegramConverterBot.Helpers;
using TelegramConverterBot.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using ItpParagraph = iText.Layout.Element.Paragraph;

namespace TelegramConverterBot.Converters;

/// <summary>
/// Converts PPTX files to PDF format using OpenXML SDK for reading and iText7 for PDF creation.
/// </summary>
public class PptxToPdfConverter : IConverter
{
    private readonly ILogger<PptxToPdfConverter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PptxToPdfConverter"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public PptxToPdfConverter(ILogger<PptxToPdfConverter> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> ConvertAsync(string inputPath, ConversionTarget target, CancellationToken cancellationToken = default)
    {
        var outputPath = FileHelper.GetOutputFilePath(
            Path.GetDirectoryName(inputPath)!,
            Path.GetFileName(inputPath),
            ".pdf");

        _logger.LogInformation("Converting PPTX to PDF: {Input} → {Output}", inputPath, outputPath);

        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Extract slide content from PPTX
            var slides = ExtractSlidesContent(inputPath);

            // Create PDF with one page per slide
            using var pdfWriter = new PdfWriter(outputPath);
            using var pdfDoc = new PdfDocument(pdfWriter);
            using var document = new Document(pdfDoc);

            for (int i = 0; i < slides.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var slide = slides[i];

                // Add slide title if present
                if (!string.IsNullOrWhiteSpace(slide.Title))
                {
                    var titlePara = new ItpParagraph(slide.Title)
                        .SetFontSize(18)
                        .SetBold();
                    document.Add(titlePara);
                }

                // Add slide content
                if (!string.IsNullOrWhiteSpace(slide.Content))
                {
                    var contentPara = new ItpParagraph(slide.Content)
                        .SetFontSize(12);
                    document.Add(contentPara);
                }

                // Add page break after each slide except the last
                if (i < slides.Count - 1)
                {
                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                }
            }
        }, cancellationToken);

        _logger.LogInformation("PPTX to PDF conversion completed: {Output}", outputPath);
        return outputPath;
    }

    /// <summary>
    /// Represents the content extracted from a single slide.
    /// </summary>
    private class SlideContent
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Extracts text content from all slides in a PPTX file.
    /// </summary>
    /// <param name="pptxPath">The path to the PPTX file.</param>
    /// <returns>A list of SlideContent objects, one per slide.</returns>
    private static List<SlideContent> ExtractSlidesContent(string pptxPath)
    {
        var slides = new List<SlideContent>();

        using (PresentationDocument presentationDoc = PresentationDocument.Open(pptxPath, false))
        {
            var presentationPart = presentationDoc.PresentationPart;
            if (presentationPart == null)
                return slides;

            var presentation = presentationPart.Presentation;
            if (presentation.SlideIdList == null)
                return slides;

            var slideIdList = presentation.SlideIdList.Elements<SlideId>().ToList();

            foreach (var slideId in slideIdList)
            {
                var relId = slideId.RelationshipId?.Value;
                if (string.IsNullOrEmpty(relId))
                    continue;

                var slidePart = (SlidePart)presentationPart.GetPartById(relId);
                var slideContent = new SlideContent();
                var textParts = new List<string>();

                var commonSlideData = slidePart.Slide.CommonSlideData;
                if (commonSlideData?.ShapeTree == null)
                    continue;

                // Extract all text from shapes on the slide
                foreach (var shape in commonSlideData.ShapeTree.Elements<Shape>())
                {
                    var textBody = shape.TextBody;
                    if (textBody == null) continue;

                    var shapeText = string.Join(" ",
                        textBody.Elements<DocumentFormat.OpenXml.Drawing.Paragraph>()
                            .Select(p => p.InnerText)
                            .Where(t => !string.IsNullOrWhiteSpace(t)));

                    if (!string.IsNullOrWhiteSpace(shapeText))
                    {
                        // Check if this is likely a title shape
                        var nonVisualProperties = shape.NonVisualShapeProperties;
                        if (nonVisualProperties != null)
                        {
                            var applicationNonVisualDrawingProps = nonVisualProperties
                                .GetFirstChild<ApplicationNonVisualDrawingProperties>();

                            if (applicationNonVisualDrawingProps != null &&
                                applicationNonVisualDrawingProps.PlaceholderShape != null)
                            {
                                var placeholderType = applicationNonVisualDrawingProps.PlaceholderShape.Type;
                                if (placeholderType != null &&
                                    placeholderType.Value == PlaceholderValues.Title)
                                {
                                    slideContent.Title = shapeText;
                                    continue;
                                }
                            }
                        }

                        textParts.Add(shapeText);
                    }
                }

                slideContent.Content = string.Join("\n", textParts);
                slides.Add(slideContent);
            }
        }

        return slides;
    }
}

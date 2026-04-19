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

        try
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Extract slide content from PPTX
                var slides = ExtractSlidesContent(inputPath);

                // Create PDF with one page per slide
                using var pdfWriter = new PdfWriter(outputPath);
                using var pdfDoc = new PdfDocument(pdfWriter);
                using var document = new Document(pdfDoc);

                // Unicode-capable font so Cyrillic/Uzbek text doesn't crash iText.
                var font = PdfFontHelper.CreateUnicodeFont();
                document.SetFont(font);

                if (slides.Count == 0)
                {
                    document.Add(new ItpParagraph("(No slides found in presentation)"));
                    return;
                }

                bool anyContentWritten = false;
                for (int i = 0; i < slides.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var slide = slides[i];
                    bool hasTitle = !string.IsNullOrWhiteSpace(slide.Title);
                    bool hasContent = !string.IsNullOrWhiteSpace(slide.Content);

                    if (hasTitle)
                    {
                        document.Add(new ItpParagraph(slide.Title)
                            .SetFontSize(18)
                            .SetBold());
                        anyContentWritten = true;
                    }

                    if (hasContent)
                    {
                        document.Add(new ItpParagraph(slide.Content).SetFontSize(12));
                        anyContentWritten = true;
                    }

                    if (!hasTitle && !hasContent)
                    {
                        // Placeholder keeps slide numbering aligned and guarantees a page exists.
                        document.Add(new ItpParagraph($"(Slide {i + 1} — no text content)")
                            .SetFontSize(10)
                            .SetItalic());
                        anyContentWritten = true;
                    }

                    if (i < slides.Count - 1)
                    {
                        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    }
                }

                if (!anyContentWritten)
                {
                    document.Add(new ItpParagraph("(Presentation contained no extractable text)"));
                }
            }, cancellationToken);

            _logger.LogInformation("PPTX to PDF conversion completed: {Output}", outputPath);
            return outputPath;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during PPTX to PDF conversion: {Input}", inputPath);
            throw new InvalidOperationException($"Failed to convert PPTX: {ex.Message}", ex);
        }
    }

    private class SlideContent
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Extracts text content from all slides in a PPTX file.
    /// </summary>
    private static List<SlideContent> ExtractSlidesContent(string pptxPath)
    {
        var slides = new List<SlideContent>();

        using var presentationDoc = PresentationDocument.Open(pptxPath, false);
        var presentationPart = presentationDoc.PresentationPart;
        if (presentationPart?.Presentation?.SlideIdList == null)
            return slides;

        foreach (var slideId in presentationPart.Presentation.SlideIdList.Elements<SlideId>())
        {
            var relId = slideId.RelationshipId?.Value;
            if (string.IsNullOrEmpty(relId))
                continue;

            if (presentationPart.GetPartById(relId) is not SlidePart slidePart)
                continue;

            var slideContent = new SlideContent();
            var textParts = new List<string>();

            var shapeTree = slidePart.Slide?.CommonSlideData?.ShapeTree;
            if (shapeTree != null)
            {
                CollectShapeText(shapeTree, slideContent, textParts);
            }

            slideContent.Content = string.Join("\n", textParts);
            slides.Add(slideContent);
        }

        return slides;
    }

    /// <summary>
    /// Walks a shape container recursively, extracting text from shapes and nested group shapes.
    /// </summary>
    private static void CollectShapeText(OpenXmlElement container, SlideContent slideContent, List<string> textParts)
    {
        foreach (var element in container.Elements())
        {
            switch (element)
            {
                case Shape shape:
                    ExtractFromShape(shape, slideContent, textParts);
                    break;
                case GroupShape group:
                    CollectShapeText(group, slideContent, textParts);
                    break;
            }
        }
    }

    private static void ExtractFromShape(Shape shape, SlideContent slideContent, List<string> textParts)
    {
        var textBody = shape.TextBody;
        if (textBody == null) return;

        var shapeText = string.Join(" ",
            textBody.Elements<A.Paragraph>()
                .Select(p => p.InnerText)
                .Where(t => !string.IsNullOrWhiteSpace(t)));

        if (string.IsNullOrWhiteSpace(shapeText)) return;

        var placeholderType = shape.NonVisualShapeProperties
            ?.GetFirstChild<ApplicationNonVisualDrawingProperties>()
            ?.PlaceholderShape?.Type;

        if (placeholderType?.Value == PlaceholderValues.Title &&
            string.IsNullOrEmpty(slideContent.Title))
        {
            slideContent.Title = shapeText;
            return;
        }

        textParts.Add(shapeText);
    }
}

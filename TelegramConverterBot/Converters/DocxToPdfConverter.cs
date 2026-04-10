using Microsoft.Extensions.Logging;
using TelegramConverterBot.Helpers;
using TelegramConverterBot.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Document = DocumentFormat.OpenXml.Wordprocessing.Document;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using ItpParagraph = iText.Layout.Element.Paragraph;

namespace TelegramConverterBot.Converters;

/// <summary>
/// Converts DOCX files to PDF format using OpenXML SDK for reading and iText7 for PDF creation.
/// </summary>
public class DocxToPdfConverter : IConverter
{
    private readonly ILogger<DocxToPdfConverter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocxToPdfConverter"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public DocxToPdfConverter(ILogger<DocxToPdfConverter> logger)
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

        _logger.LogInformation("Converting DOCX to PDF: {Input} → {Output}", inputPath, outputPath);

        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Read DOCX content using OpenXML
            var paragraphs = ExtractTextFromDocx(inputPath);

            // Create PDF using iText7
            using var pdfWriter = new PdfWriter(outputPath);
            using var pdfDoc = new PdfDocument(pdfWriter);
            using var doc = new iText.Layout.Document(pdfDoc);

            foreach (var para in paragraphs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(para))
                {
                    // Add empty line
                    doc.Add(new ItpParagraph(" "));
                }
                else
                {
                    doc.Add(new ItpParagraph(para));
                }
            }
        }, cancellationToken);

        _logger.LogInformation("DOCX to PDF conversion completed: {Output}", outputPath);
        return outputPath;
    }

    /// <summary>
    /// Extracts text paragraphs from a DOCX file.
    /// </summary>
    /// <param name="docxPath">The path to the DOCX file.</param>
    /// <returns>A list of text paragraphs extracted from the document.</returns>
    private static List<string> ExtractTextFromDocx(string docxPath)
    {
        var paragraphs = new List<string>();

        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(docxPath, false))
        {
            var body = wordDoc.MainDocumentPart?.Document.Body;
            if (body == null)
                return paragraphs;

            foreach (var paragraph in body.Elements<Paragraph>())
            {
                var text = paragraph.InnerText;
                paragraphs.Add(text);
            }
        }

        return paragraphs;
    }
}

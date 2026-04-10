using Microsoft.Extensions.Logging;
using TelegramConverterBot.Helpers;
using TelegramConverterBot.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace TelegramConverterBot.Converters;

/// <summary>
/// Converts PDF files to DOCX format using iText7 for reading and OpenXML SDK for DOCX creation.
/// </summary>
public class PdfToDocxConverter : IConverter
{
    private readonly ILogger<PdfToDocxConverter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfToDocxConverter"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public PdfToDocxConverter(ILogger<PdfToDocxConverter> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> ConvertAsync(string inputPath, ConversionTarget target, CancellationToken cancellationToken = default)
    {
        var outputPath = FileHelper.GetOutputFilePath(
            Path.GetDirectoryName(inputPath)!,
            Path.GetFileName(inputPath),
            ".docx");

        _logger.LogInformation("Converting PDF to DOCX: {Input} → {Output}", inputPath, outputPath);

        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Extract text from PDF using iText7
            var pageTexts = ExtractTextFromPdf(inputPath);

            // Create DOCX using OpenXML SDK
            CreateDocxFromText(outputPath, pageTexts);
        }, cancellationToken);

        _logger.LogInformation("PDF to DOCX conversion completed: {Output}", outputPath);
        return outputPath;
    }

    /// <summary>
    /// Extracts text from each page of a PDF document.
    /// </summary>
    /// <param name="pdfPath">The path to the PDF file.</param>
    /// <returns>A list of text content, one element per page.</returns>
    private static List<string> ExtractTextFromPdf(string pdfPath)
    {
        var pageTexts = new List<string>();

        using var pdfReader = new PdfReader(pdfPath);
        using var pdfDoc = new PdfDocument(pdfReader);

        int pageCount = pdfDoc.GetNumberOfPages();

        for (int i = 1; i <= pageCount; i++)
        {
            var strategy = new LocationTextExtractionStrategy();
            var page = pdfDoc.GetPage(i);
            var text = PdfTextExtractor.GetTextFromPage(page, strategy);
            pageTexts.Add(text);
        }

        return pageTexts;
    }

    /// <summary>
    /// Creates a DOCX file from a list of text strings (one per page).
    /// </summary>
    /// <param name="docxPath">The output path for the DOCX file.</param>
    /// <param name="pageTexts">The text content for each page.</param>
    private static void CreateDocxFromText(string docxPath, List<string> pageTexts)
    {
        using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(
            docxPath,
            WordprocessingDocumentType.Document))
        {
            // Add a main document part
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
            var body = mainPart.Document.AppendChild(new Body());

            foreach (var pageText in pageTexts)
            {
                if (string.IsNullOrWhiteSpace(pageText))
                    continue;

                // Split text into lines and create paragraphs
                var lines = pageText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        // Add empty paragraph
                        body.AppendChild(new Paragraph());
                    }
                    else
                    {
                        var para = new Paragraph();
                        var run = new Run();
                        run.AppendChild(new Text(line));
                        para.AppendChild(run);
                        body.AppendChild(para);
                    }
                }

                // Add page break between pages (except after the last one)
                if (pageTexts.IndexOf(pageText) < pageTexts.Count - 1)
                {
                    var breakPara = new Paragraph();
                    var breakRun = new Run();
                    breakRun.AppendChild(new Break() { Type = BreakValues.Page });
                    breakPara.AppendChild(breakRun);
                    body.AppendChild(breakPara);
                }
            }

            mainPart.Document.Save();
        }
    }
}

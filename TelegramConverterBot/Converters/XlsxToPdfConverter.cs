using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using TelegramConverterBot.Helpers;
using TelegramConverterBot.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using ItpParagraph = iText.Layout.Element.Paragraph;

namespace TelegramConverterBot.Converters;

/// <summary>
/// Converts XLSX files to PDF format using EPPlus for reading and iText7 for PDF creation.
/// </summary>
public class XlsxToPdfConverter : IConverter
{
    private readonly ILogger<XlsxToPdfConverter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="XlsxToPdfConverter"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public XlsxToPdfConverter(ILogger<XlsxToPdfConverter> logger)
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

        _logger.LogInformation("Converting XLSX to PDF: {Input} → {Output}", inputPath, outputPath);

        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Read XLSX content using EPPlus
            var sheets = ReadExcelContent(inputPath);

            // Create PDF using iText7
            using var pdfWriter = new PdfWriter(outputPath);
            using var pdfDoc = new PdfDocument(pdfWriter);
            using var document = new Document(pdfDoc);

            foreach (var sheet in sheets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Add sheet name as header
                var header = new ItpParagraph(sheet.SheetName)
                    .SetFontSize(16)
                    .SetBold()
                    .SetMarginBottom(10);
                document.Add(header);

                if (sheet.Rows.Count == 0)
                {
                    document.Add(new ItpParagraph("Empty sheet")
                        .SetFontSize(10)
                        .SetItalic());
                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    continue;
                }

                // Create table with appropriate number of columns
                int maxCols = sheet.Rows.Max(r => r.Count);
                var table = new Table(UnitValue.CreatePercentArray(maxCols)).UseAllAvailableWidth();

                // Add header row styling (first row)
                bool isHeader = true;
                foreach (var row in sheet.Rows)
                {
                    for (int col = 0; col < maxCols; col++)
                    {
                        var cellValue = col < row.Count ? row[col] : string.Empty;
                        var cell = new Cell().Add(new ItpParagraph(cellValue).SetFontSize(9));

                        if (isHeader)
                        {
                            cell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                            cell.SetFontColor(ColorConstants.BLACK);
                            cell.SetBold();
                        }

                        table.AddCell(cell);
                    }
                    isHeader = false;
                }

                document.Add(table);
                document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
            }
        }, cancellationToken);

        _logger.LogInformation("XLSX to PDF conversion completed: {Output}", outputPath);
        return outputPath;
    }

    /// <summary>
    /// Represents the content of a single Excel sheet.
    /// </summary>
    private class SheetContent
    {
        public string SheetName { get; set; } = string.Empty;
        public List<List<string>> Rows { get; set; } = new();
    }

    /// <summary>
    /// Reads all sheets and their content from an Excel file.
    /// </summary>
    /// <param name="xlsxPath">The path to the XLSX file.</param>
    /// <returns>A list of SheetContent objects, one per sheet.</returns>
    private static List<SheetContent> ReadExcelContent(string xlsxPath)
    {
        var sheets = new List<SheetContent>();

        using var package = new ExcelPackage(new FileInfo(xlsxPath));

        foreach (var worksheet in package.Workbook.Worksheets)
        {
            var sheetContent = new SheetContent
            {
                SheetName = worksheet.Name
            };

            int rowCount = worksheet.Dimension?.Rows ?? 0;
            int colCount = worksheet.Dimension?.Columns ?? 0;

            if (rowCount == 0 || colCount == 0)
                continue;

            for (int row = 1; row <= rowCount; row++)
            {
                var rowData = new List<string>();
                for (int col = 1; col <= colCount; col++)
                {
                    var cell = worksheet.Cells[row, col];
                    rowData.Add(cell.Text ?? string.Empty);
                }
                sheetContent.Rows.Add(rowData);
            }

            sheets.Add(sheetContent);
        }

        return sheets;
    }
}

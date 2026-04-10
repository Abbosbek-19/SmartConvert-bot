using Microsoft.Extensions.Logging;
using TelegramConverterBot.Helpers;
using TelegramConverterBot.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;

namespace TelegramConverterBot.Converters;

/// <summary>
/// Converts image files (JPG, PNG) to PDF format using iText7.
/// </summary>
public class ImageToPdfConverter : IConverter
{
    private readonly ILogger<ImageToPdfConverter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageToPdfConverter"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ImageToPdfConverter(ILogger<ImageToPdfConverter> logger)
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

        _logger.LogInformation("Converting Image to PDF: {Input} → {Output}", inputPath, outputPath);

        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create PDF with image scaled to fit A4 page
            using var pdfWriter = new PdfWriter(outputPath);
            using var pdfDoc = new PdfDocument(pdfWriter);
            using var document = new Document(pdfDoc);

            // Create image data from file
            var imageData = ImageDataFactory.Create(inputPath);

            // A4 page dimensions in points (72 points per inch)
            // A4: 595 x 842 points
            float pageWidth = pdfDoc.GetDefaultPageSize().GetWidth() - 72; // 1 inch margins
            float pageHeight = pdfDoc.GetDefaultPageSize().GetHeight() - 72;

            float imageWidth = imageData.GetWidth();
            float imageHeight = imageData.GetHeight();

            // Scale image to fit page while maintaining aspect ratio
            float scaleWidth = pageWidth / imageWidth;
            float scaleHeight = pageHeight / imageHeight;
            float scale = Math.Min(scaleWidth, scaleHeight);

            float scaledWidth = imageWidth * scale;
            float scaledHeight = imageHeight * scale;

            // Create image element and add to document
            var image = new Image(imageData)
                .ScaleToFit(scaledWidth, scaledHeight)
                .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

            document.Add(image);
        }, cancellationToken);

        _logger.LogInformation("Image to PDF conversion completed: {Output}", outputPath);
        return outputPath;
    }
}

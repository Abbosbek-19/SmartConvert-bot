using Microsoft.Extensions.Logging;
using TelegramConverterBot.Converters;
using TelegramConverterBot.Models;

namespace TelegramConverterBot.Services;

/// <summary>
/// Routes conversion jobs to the appropriate converter implementation.
/// </summary>
public class ConversionService
{
    private readonly ILogger<ConversionService> _logger;
    private readonly DocxToPdfConverter _docxToPdfConverter;
    private readonly PdfToDocxConverter _pdfToDocxConverter;
    private readonly PptxToPdfConverter _pptxToPdfConverter;
    private readonly XlsxToPdfConverter _xlsxToPdfConverter;
    private readonly ImageToPdfConverter _imageToPdfConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversionService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="docxToPdfConverter">The DOCX to PDF converter.</param>
    /// <param name="pdfToDocxConverter">The PDF to DOCX converter.</param>
    /// <param name="pptxToPdfConverter">The PPTX to PDF converter.</param>
    /// <param name="xlsxToPdfConverter">The XLSX to PDF converter.</param>
    /// <param name="imageToPdfConverter">The Image to PDF converter.</param>
    public ConversionService(
        ILogger<ConversionService> logger,
        DocxToPdfConverter docxToPdfConverter,
        PdfToDocxConverter pdfToDocxConverter,
        PptxToPdfConverter pptxToPdfConverter,
        XlsxToPdfConverter xlsxToPdfConverter,
        ImageToPdfConverter imageToPdfConverter)
    {
        _logger = logger;
        _docxToPdfConverter = docxToPdfConverter;
        _pdfToDocxConverter = pdfToDocxConverter;
        _pptxToPdfConverter = pptxToPdfConverter;
        _xlsxToPdfConverter = xlsxToPdfConverter;
        _imageToPdfConverter = imageToPdfConverter;
    }

    /// <summary>
    /// Executes a conversion job and returns the path to the converted file.
    /// </summary>
    /// <param name="job">The conversion job containing source file information.</param>
    /// <param name="target">The target format to convert to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The full path to the converted output file.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no converter is available for the given file type combination.</exception>
    public async Task<string> ConvertAsync(ConversionJob job, ConversionTarget target, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting conversion: {SourceType} → {Target} for file {FileName}",
            job.DetectedType, target, job.OriginalFileName);

        try
        {
            IConverter converter = GetConverter(job.DetectedType, target);
            var outputPath = await converter.ConvertAsync(job.TempInputPath, target, cancellationToken);

            _logger.LogInformation(
                "Conversion completed: {SourceType} → {Target}, output: {OutputPath}",
                job.DetectedType, target, outputPath);

            return outputPath;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Conversion was cancelled for {FileName}", job.OriginalFileName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Conversion failed: {SourceType} → {Target} for file {FileName}",
                job.DetectedType, target, job.OriginalFileName);
            throw new InvalidOperationException(
                $"Failed to convert {job.DetectedType} to {target}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the appropriate converter instance for the given source and target formats.
    /// </summary>
    /// <param name="source">The source file type.</param>
    /// <param name="target">The target conversion format.</param>
    /// <returns>An IConverter instance capable of performing the conversion.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no converter supports the given combination.</exception>
    private IConverter GetConverter(FileType source, ConversionTarget target)
    {
        return (source, target) switch
        {
            (FileType.Docx, ConversionTarget.Pdf) => _docxToPdfConverter,
            (FileType.Pdf, ConversionTarget.Docx) => _pdfToDocxConverter,
            (FileType.Pptx, ConversionTarget.Pdf) => _pptxToPdfConverter,
            (FileType.Xlsx, ConversionTarget.Pdf) => _xlsxToPdfConverter,
            (FileType.Image, ConversionTarget.Pdf) => _imageToPdfConverter,
            _ => throw new InvalidOperationException($"No converter available for {source} → {target}")
        };
    }
}

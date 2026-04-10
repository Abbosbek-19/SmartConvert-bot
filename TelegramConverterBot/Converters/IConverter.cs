using TelegramConverterBot.Models;

namespace TelegramConverterBot.Converters;

/// <summary>
/// Defines the interface for all file converters.
/// </summary>
public interface IConverter
{
    /// <summary>
    /// Converts a file from one format to another.
    /// </summary>
    /// <param name="inputPath">The full path to the input file.</param>
    /// <param name="target">The target format to convert to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The full path to the converted output file.</returns>
    Task<string> ConvertAsync(string inputPath, ConversionTarget target, CancellationToken cancellationToken = default);
}

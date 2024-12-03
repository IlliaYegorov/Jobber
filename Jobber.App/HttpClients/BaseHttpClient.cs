using System.IO.Compression;
using System.Text;

namespace Jobber.App.HttpClients;

public abstract class BaseHttpClient
{
    protected readonly HttpClient HttpClient;

    protected BaseHttpClient(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <summary>
    /// Sends an HTTP GET request to the specified URL and returns the decompressed HTML content as a string.
    /// </summary>
    /// <param name="uri">The URL to send the request to.</param>
    /// <returns>The decompressed HTML content as a string.</returns>
    protected async Task<string> GetDecompressedHtmlAsync(Uri uri)
    {
        var response = await HttpClient.GetAsync(uri);
        response.EnsureSuccessStatusCode();

        using var responseStream = await response.Content.ReadAsStreamAsync();
        var decompressedStream = DecompressStreamIfNeeded(responseStream, response.Content.Headers.ContentEncoding);

        using var reader = new StreamReader(decompressedStream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Decompresses the stream if the content encoding is gzip or deflate.
    /// </summary>
    /// <param name="responseStream">The original response stream.</param>
    /// <param name="contentEncoding">The content encoding headers as a collection of strings.</param>
    /// <returns>The decompressed stream.</returns>
    private static Stream DecompressStreamIfNeeded(Stream responseStream, ICollection<string> contentEncoding)
    {
        if (contentEncoding.Contains("gzip"))
        {
            return new GZipStream(responseStream, CompressionMode.Decompress);
        }
        else if (contentEncoding.Contains("deflate"))
        {
            return new DeflateStream(responseStream, CompressionMode.Decompress);
        }

        return responseStream;
    }
}

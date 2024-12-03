using System.Web;

namespace Jobber.App.HttpClients;

public interface IUpworkHttpClient
{
    public Task<string> GetHtmlAsync(string searchQuery);
}

public class UpworkHttpClient : BaseHttpClient, IUpworkHttpClient
{
    private readonly HttpClient _httpClient;

    public UpworkHttpClient(HttpClient httpClient) : base(httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetHtmlAsync(string searchQuery)
    {
        var queryParams = HttpUtility.ParseQueryString(searchQuery);
        var encodedQuery = string.Join("&",
            queryParams.AllKeys.Select(key => $"{key}={HttpUtility.UrlEncode(queryParams[key])}")
        );

        var builder = new UriBuilder(_httpClient.BaseAddress)
        {
            Query = encodedQuery
        };

        var html = await GetDecompressedHtmlAsync(builder.Uri);

        return html;
    }

}

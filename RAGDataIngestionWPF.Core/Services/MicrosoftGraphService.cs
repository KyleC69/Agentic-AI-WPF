// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF.Core
//  File:         MicrosoftGraphService.cs
//   Author: Kyle L. Crowder



using Newtonsoft.Json;

using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Helpers;
using RAGDataIngestionWPF.Core.Models;




namespace RAGDataIngestionWPF.Core.Services;





public class MicrosoftGraphService : IMicrosoftGraphService
{
    private readonly HttpClient _client;
    /*
    For more information about Get-User Service, refer to the following documentation
    https://docs.microsoft.com/graph/api/user-get?view=graph-rest-1.0
    You can test calls to the Microsoft Graph with the Microsoft Graph Explorer
    https://developer.microsoft.com/graph/graph-explorer
    */

    private const string _apiServiceMe = "me/";
    private const string _apiServiceMePhoto = "me/photo/$value";








    public MicrosoftGraphService(IHttpClientFactory client)
    {
        _client = client.CreateClient("msgraph");
    }








    public async Task<User> GetUserInfoAsync(string accessToken)
    {
        User user = null;
        HttpContent httpContent = await GetDataAsync($"{_apiServiceMe}", accessToken);
        if (httpContent != null)
        {
            string userData = await httpContent.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(userData))
            {
                user = JsonConvert.DeserializeObject<User>(userData);
            }
        }

        return user;
    }








    public async Task<string> GetUserPhoto(string accessToken)
    {
        HttpContent httpContent = await GetDataAsync($"{_apiServiceMePhoto}", accessToken);

        if (httpContent == null)
        {
            return string.Empty;
        }

        Stream stream = await httpContent.ReadAsStreamAsync();
        return stream.ToBase64String();
    }








    private async Task<HttpContent> GetDataAsync(string url, string accessToken)
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Get, url);
            request.Headers.Authorization = new("Bearer", accessToken);
            HttpResponseMessage response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return response.Content;
            }
            // TODO: Please handle other status codes as appropriate to your scenario
        }
        catch (HttpRequestException)
        {
            // TODO: The request failed due to an underlying issue such as
            // network connectivity, DNS failure, server certificate validation or timeout.
            // Please handle this exception as appropriate to your scenario
        }
        catch (Exception)
        {
            // TODO: This call can fail please handle exceptions as appropriate to your scenario
        }

        return null;
    }
}
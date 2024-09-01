using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Scenarios.AzureB2C.BlazorClient;

public sealed class ApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<string> GetGreeting()
    {
        var response = await _httpClient.GetAsync("greeting");
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> GetSecureGreeting()
    {
        try
        {
            var response = await _httpClient.GetAsync("greeting-secure");
            return await response.Content.ReadAsStringAsync();
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            return "";
        }
    }
}
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Scenarios.AzureB2C.BlazorClient;

public sealed class ApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<UserClaim[]> GetClaims()
    {
        try
        {
            var response = await _httpClient.GetAsync("claims");
            return await response.Content.ReadFromJsonAsync<UserClaim[]>() ?? [];
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            return [];
        }
    }
}
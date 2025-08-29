using McpMesh.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace McpMesh.Services;

public interface IAuthService
{
    Task<bool> ValidateAsync(string username, string password);
}

public class AuthService : IAuthService
{
    private readonly IOptions<McpMeshOptions> _options;
    private readonly HttpClient _httpClient;

    public AuthService(IOptions<McpMeshOptions> options, HttpClient httpClient)
    {
        _options = options;
        _httpClient = httpClient;
    }

    public async Task<bool> ValidateAsync(string username, string password)
    {
        var authenticationLoginRequest = _options.Value.Authentication.LoginRequest;
        var method = new HttpMethod(authenticationLoginRequest.Method);

        var message = new HttpRequestMessage(method, authenticationLoginRequest.Url)
        {
            Content = new StringContent(
                authenticationLoginRequest.BodyTemplate
                    .Replace("{username}", username)
                    .Replace("{password}", password), MediaTypeHeaderValue.Parse(authenticationLoginRequest.ContentType))
        };

        foreach (var header in authenticationLoginRequest.Headers)
        {
            message.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        var response = await _httpClient.SendAsync(message);
        return response.IsSuccessStatusCode;
    }
}
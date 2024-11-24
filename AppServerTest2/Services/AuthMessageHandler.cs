using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AppServerTest2.Services;

public class AuthMessageHandler : DelegatingHandler
{
    private readonly SecureStorageService _secureStorageService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthMessageHandler> _logger;

    public AuthMessageHandler(
        SecureStorageService secureStorageService,
        IHttpClientFactory httpClientFactory,
        ILogger<AuthMessageHandler> logger)
    {
        _secureStorageService = secureStorageService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Log the outgoing request URL
        _logger.LogInformation("Sending request to {Url}", request.RequestUri);

        // Add access token if available
        var accessToken = await _secureStorageService.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        // Send the request
        var response = await base.SendAsync(request, cancellationToken);

        // Log response status
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Request to {Url} succeeded with status code {StatusCode}", request.RequestUri, response.StatusCode);
        }
        else
        {
            _logger.LogWarning("Request to {Url} failed with status code {StatusCode}", request.RequestUri, response.StatusCode);
        }

        // Handle Unauthorized responses (401)
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Unauthorized response for request to {Url}. Attempting to refresh token.", request.RequestUri);

            // Attempt to refresh token
            var isTokenRefreshed = await RefreshTokenAsync();

            if (isTokenRefreshed)
            {
                _logger.LogInformation("Token refreshed successfully. Retrying the request.");

                // Retry the request with the new access token
                accessToken = await _secureStorageService.GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }

                response.Dispose(); // Dispose the original response
                response = await base.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Retried request to {Url} succeeded with status code {StatusCode}", request.RequestUri, response.StatusCode);
                }
                else
                {
                    _logger.LogWarning("Retried request to {Url} failed with status code {StatusCode}", request.RequestUri, response.StatusCode);
                }
            }
            else
            {
                _logger.LogError("Token refresh failed. Logging out the user and removing tokens.");
                _secureStorageService.RemoveTokens();
                // Optionally navigate to login page
            }
        }

        return response;
    }

    private async Task<bool> RefreshTokenAsync()
    {
        var refreshToken = await _secureStorageService.GetRefreshTokenAsync();

        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("No refresh token available. Unable to refresh access token.");
            return false;
        }

        _logger.LogInformation("Attempting to refresh token.");

        var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshToken };

        try
        {
            var client = _httpClientFactory.CreateClient("AuthorizedClient");
            var response = await client.PostAsJsonAsync("refresh-token", refreshRequest);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Token refresh request succeeded.");
                var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

                await _secureStorageService.SetAccessTokenAsync(refreshResponse!.AccessToken);
                await _secureStorageService.SetRefreshTokenAsync(refreshResponse.RefreshToken);

                return true;
            }
            else
            {
                _logger.LogWarning("Token refresh request failed with status code {StatusCode}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while refreshing token: {Message}", ex.Message);
            return false;
        }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

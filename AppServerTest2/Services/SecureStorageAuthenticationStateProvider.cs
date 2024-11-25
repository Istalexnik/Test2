using System.Security.Claims;
using AppServerTest2.Services;
using Microsoft.AspNetCore.Components.Authorization;

public class SecureStorageAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly SecureStorageService _secureStorageService;

    public SecureStorageAuthenticationStateProvider(SecureStorageService secureStorageService)
    {
        _secureStorageService = secureStorageService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var accessToken = await _secureStorageService.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(accessToken))
        {
            // Create authenticated user
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "AuthenticatedUser") }, "Bearer");
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        // Create unauthenticated state
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}

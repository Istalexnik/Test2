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

    // Method to notify that the authentication state has changed
    public void NotifyUserAuthentication()
    {
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "AuthenticatedUser") }, "Bearer"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }
}

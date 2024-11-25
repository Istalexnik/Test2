public class SecureStorageService
{
    public async Task<string?> GetAccessTokenAsync()
    {
        return await SecureStorage.GetAsync("AccessToken");
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await SecureStorage.GetAsync("RefreshToken");
    }

    public async Task<bool> IsAccessTokenValidAsync()
    {
        var accessToken = await GetAccessTokenAsync();
        return !string.IsNullOrEmpty(accessToken) && ValidateToken(accessToken);
    }

    private bool ValidateToken(string token)
    {
        // Add logic to validate the token (e.g., check expiry)
        return true; // Replace with real validation logic
    }

    public async Task SetAccessTokenAsync(string token)
    {
        await SecureStorage.SetAsync("AccessToken", token);
    }

    public async Task SetRefreshTokenAsync(string token)
    {
        await SecureStorage.SetAsync("RefreshToken", token);
    }

    public void RemoveTokens()
    {
        SecureStorage.Remove("AccessToken");
        SecureStorage.Remove("RefreshToken");
    }
}

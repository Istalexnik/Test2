using Microsoft.Extensions.Logging;
using AppServerTest2.Services;


namespace AppServerTest2;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        // Register SecureStorageService
        builder.Services.AddSingleton<SecureStorageService>();

        // Register AuthMessageHandler
        builder.Services.AddTransient<AuthMessageHandler>();

        builder.Services.AddSingleton<IAlertService, AlertService>();


#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<HttpClient>(sp => new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.GetBaseUrl())
        });

        // Configure HttpClient with AuthMessageHandler
        builder.Services.AddHttpClient("AuthorizedClient", client =>
        {
            client.BaseAddress = new Uri(ApiConfig.GetBaseUrl()); // Base URL for your API
        })
        .AddHttpMessageHandler<AuthMessageHandler>();




        return builder.Build();
    }
}

public static class ApiConfig
{
    public static string GetBaseUrl()
    {
#if ANDROID 
           return "https://10.0.2.2:7078/"; 
#else
        return "https://localhost:7078/"; //return "https://192.168.50.238:7078/"; 
#endif
    }
}


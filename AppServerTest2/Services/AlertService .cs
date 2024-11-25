namespace AppServerTest2.Services;
public class AlertService : IAlertService
{
    public async Task ShowAlertAsync(string title, string message, string cancelButton)
    {
        var currentPage = Application.Current?.Windows.FirstOrDefault()?.Page as Page;
        if (currentPage != null)
        {
            await currentPage.DisplayAlert(title, message, cancelButton);
        }
    }
}
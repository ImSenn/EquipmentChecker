using System.Net.Http;
using System.Windows;
using CheckerWPF.Services;
using CheckerWPF.ViewModels;
using CheckerWPF.Views;

namespace CheckerWPF;

public partial class App : Application
{
    private const string BaseUrl = "http://localhost:5250/";

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        var auth = new AuthService(http);
        var signalR = new SignalRService(BaseUrl);
        var api = new ApiService(http);

        var loginWin = new LoginWindow();

        var loginVm = new LoginViewModel(auth, signalR, onLoginSuccess: () =>
        {
            var detailVm = new DeviceDetailViewModel(api, signalR);
            var listVm = new DeviceListViewModel(api, signalR, async device =>
            {
                await detailVm.LeaveAsync();
                await detailVm.InitAsync(device);
            });
            var mainVm = new MainViewModel(api, auth, signalR, listVm, detailVm);
            var mainWin = new MainWindow { DataContext = mainVm };

            _ = mainVm.InitAsync();

            mainWin.Show();
            loginWin.Close();
        });

        loginWin.DataContext = loginVm;
        loginWin.Show();
    }
}


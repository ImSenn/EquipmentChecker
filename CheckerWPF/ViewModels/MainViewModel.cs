using CheckerWPF.Services;

namespace CheckerWPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly SignalRService _signalR;
        private readonly AuthService _auth;

        private BaseViewModel? _currentView;
        private string _connectionStatus = "Disconnected";
        private string _currentUser = string.Empty;

        public BaseViewModel? CurrentView
        {
            get => _currentView;
            set => SetField(ref _currentView, value);
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetField(ref _connectionStatus, value);
        }

        public string CurrentUser
        {
            get => _currentUser;
            set => SetField(ref _currentUser, value);
        }

        public DeviceListViewModel DeviceListVM  { get; }
        public DeviceDetailViewModel DeviceDetailVM { get; }

        public RelayCommand ShowDeviceListCommand { get; }

        public MainViewModel(
            ApiService api,
            AuthService auth,
            SignalRService signalR,
            DeviceListViewModel deviceListVM,
            DeviceDetailViewModel deviceDetailVM)
        {
            _auth = auth;
            _signalR = signalR;
            DeviceListVM   = deviceListVM;
            DeviceDetailVM = deviceDetailVM;

            CurrentUser = auth.Username ?? string.Empty;

            ShowDeviceListCommand = new RelayCommand(() => NavigateTo(DeviceListVM));

            // Khi chọn thiết bị từ list → chuyển sang detail
            deviceListVM = new DeviceListViewModel(api, signalR, async device =>
            {
                await DeviceDetailVM.LeaveAsync();
                await DeviceDetailVM.InitAsync(device);
                NavigateTo(DeviceDetailVM);
            });

            _signalR.ConnectionStateChanged += s =>
                System.Windows.Application.Current.Dispatcher.Invoke(
                    () => ConnectionStatus = s);
        }

        public async Task InitAsync()
        {
            NavigateTo(DeviceListVM);
            await DeviceListVM.LoadDevicesAsync();
        }

        private void NavigateTo(BaseViewModel vm) => CurrentView = vm;
    }
}

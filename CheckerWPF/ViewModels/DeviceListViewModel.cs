using System.Collections.ObjectModel;
using System.Windows;
using CheckerWPF.Models;
using CheckerWPF.Services;

namespace CheckerWPF.ViewModels
{
    // Model bọc DeviceDto để thêm trạng thái UI
    public class DeviceItem : BaseViewModel
    {
        private bool _isOnline;
        private string _runState = "UNKNOWN";
        private double _lastTemperature;

        public string DeviceId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public double PowerRating { get; init; }

        public bool IsOnline
        {
            get => _isOnline;
            set => SetField(ref _isOnline, value);
        }

        public string RunState
        {
            get => _runState;
            set => SetField(ref _runState, value);
        }

        public double LastTemperature
        {
            get => _lastTemperature;
            set => SetField(ref _lastTemperature, value);
        }

        public static DeviceItem FromDto(DeviceDto dto) => new()
        {
            DeviceId = dto.DeviceId,
            Name = dto.Name,
            Type = dto.Type,
            PowerRating = dto.PowerRating
        };
    }

    public class DeviceListViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly SignalRService _signalR;
        private readonly Action<DeviceItem> _onDeviceSelected;

        private bool _isLoading;
        private string _statusText = string.Empty;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetField(ref _isLoading, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetField(ref _statusText, value);
        }

        public ObservableCollection<DeviceItem> Devices { get; } = new();

        public AsyncRelayCommand RefreshCommand { get; }
        public RelayCommand SelectDeviceCommand { get; }

        public DeviceListViewModel(
            ApiService api,
            SignalRService signalR,
            Action<DeviceItem> onDeviceSelected)
        {
            _api = api;
            _signalR = signalR;
            _onDeviceSelected = onDeviceSelected;

            RefreshCommand = new AsyncRelayCommand(LoadDevicesAsync);
            SelectDeviceCommand = new RelayCommand(p =>
            {
                if (p is DeviceItem item) _onDeviceSelected(item);
            });

            // Cập nhật trạng thái realtime khi nhận telemetry
            _signalR.TelemetryReceived += OnTelemetryReceived;
        }

        public async Task LoadDevicesAsync()
        {
            IsLoading = true;
            StatusText = "Đang tải...";
            try
            {
                var list = await _api.GetDevicesAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Devices.Clear();
                    foreach (var d in list ?? [])
                        Devices.Add(DeviceItem.FromDto(d));
                });
                StatusText = $"{Devices.Count} thiết bị";
            }
            catch (Exception ex)
            {
                StatusText = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnTelemetryReceived(TelemetryDto dto)
        {
            var device = Devices.FirstOrDefault(d => d.DeviceId == dto.DeviceId);
            if (device is null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                device.IsOnline = true;
                device.RunState = dto.Status.RunState;
                device.LastTemperature = dto.Metrics.Temperature;
            });
        }
    }
}

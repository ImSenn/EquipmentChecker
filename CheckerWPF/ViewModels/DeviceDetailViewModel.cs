using System.Collections.ObjectModel;
using System.Windows;
using CheckerWPF.Models;
using CheckerWPF.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace CheckerWPF.ViewModels
{
    public class AlertItem
    {
        public string DeviceId { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string Severity { get; init; } = string.Empty;
        public double Value { get; init; }
        public string Message { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
        public string SeverityColor => Severity switch
        {
            "CRITICAL" => "#E24B4A",
            "HIGH"     => "#EF9F27",
            _          => "#378ADD"
        };
    }

    public class DeviceDetailViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly SignalRService _signalR;
        private DeviceItem? _device;
        private const int MaxPoints = 60; // giữ 60 điểm gần nhất trên chart

        // ── Live metrics ──────────────────────────────────────────────────
        private double _currentTemp;
        private double _currentVibration;
        private double _currentPower;
        private string _runState = "—";
        private string _powerState = "—";
        private double _wifiRssi;
        private double _uptime;

        public double CurrentTemp      { get => _currentTemp;      set => SetField(ref _currentTemp, value); }
        public double CurrentVibration { get => _currentVibration; set => SetField(ref _currentVibration, value); }
        public double CurrentPower     { get => _currentPower;     set => SetField(ref _currentPower, value); }
        public string RunState         { get => _runState;         set => SetField(ref _runState, value); }
        public string PowerState       { get => _powerState;       set => SetField(ref _powerState, value); }
        public double WifiRssi         { get => _wifiRssi;         set => SetField(ref _wifiRssi, value); }
        public double Uptime           { get => _uptime;           set => SetField(ref _uptime, value); }

        // ── Energy ────────────────────────────────────────────────────────
        private double _energyKWh;
        private double _estimatedCost;
        public double EnergyKWh      { get => _energyKWh;      set => SetField(ref _energyKWh, value); }
        public double EstimatedCost  { get => _estimatedCost;  set => SetField(ref _estimatedCost, value); }

        // ── Status ────────────────────────────────────────────────────────
        private string _statusText = string.Empty;
        public string StatusText { get => _statusText; set => SetField(ref _statusText, value); }

        public string DeviceName => _device?.Name ?? "—";
        public string DeviceId   => _device?.DeviceId ?? "—";

        // ── Charts (LiveCharts2) ──────────────────────────────────────────
        public ObservableCollection<double> TempValues  { get; } = new();
        public ObservableCollection<double> VibValues   { get; } = new();
        public ObservableCollection<double> PowerValues { get; } = new();

        public ISeries[] TempSeries  { get; }
        public ISeries[] VibSeries   { get; }
        public ISeries[] PowerSeries { get; }

        // ── Alerts ────────────────────────────────────────────────────────
        public ObservableCollection<AlertItem> Alerts { get; } = new();

        // ── Events history ────────────────────────────────────────────────
        public ObservableCollection<EventDto> Events { get; } = new();

        // ── Commands ──────────────────────────────────────────────────────
        public AsyncRelayCommand LoadHistoryCommand  { get; }
        public AsyncRelayCommand LoadEnergyCommand   { get; }
        public AsyncRelayCommand LoadEventsCommand   { get; }
        public AsyncRelayCommand SendRestartCommand  { get; }
        public AsyncRelayCommand SendStopCommand     { get; }
        public AsyncRelayCommand SendStartCommand    { get; }

        public DeviceDetailViewModel(ApiService api, SignalRService signalR)
        {
            _api = api;
            _signalR = signalR;

            // Khởi tạo series cho LiveCharts
            TempSeries = [new LineSeries<double>
            {
                Values = TempValues,
                Name = "Nhiệt độ (°C)",
                Stroke = new SolidColorPaint(SKColor.Parse("#E24B4A"), 2),
                Fill = null,
                GeometrySize = 0
            }];

            VibSeries = [new LineSeries<double>
            {
                Values = VibValues,
                Name = "Rung động",
                Stroke = new SolidColorPaint(SKColor.Parse("#378ADD"), 2),
                Fill = null,
                GeometrySize = 0
            }];

            PowerSeries = [new LineSeries<double>
            {
                Values = PowerValues,
                Name = "Công suất (W)",
                Stroke = new SolidColorPaint(SKColor.Parse("#EF9F27"), 2),
                Fill = null,
                GeometrySize = 0
            }];

            LoadHistoryCommand = new AsyncRelayCommand(LoadHistoryAsync);
            LoadEnergyCommand  = new AsyncRelayCommand(LoadEnergyAsync);
            LoadEventsCommand  = new AsyncRelayCommand(LoadEventsAsync);
            SendRestartCommand = new AsyncRelayCommand(() => SendCommandAsync("RESTART"));
            SendStopCommand    = new AsyncRelayCommand(() => SendCommandAsync("STOP"));
            SendStartCommand   = new AsyncRelayCommand(() => SendCommandAsync("START"));

            _signalR.TelemetryReceived += OnTelemetryReceived;
            _signalR.AlertReceived     += OnAlertReceived;
        }

        public async Task InitAsync(DeviceItem device)
        {
            _device = device;
            OnPropertyChanged(nameof(DeviceName));
            OnPropertyChanged(nameof(DeviceId));

            TempValues.Clear();
            VibValues.Clear();
            PowerValues.Clear();
            Alerts.Clear();
            Events.Clear();

            // Join SignalR group
            await _signalR.JoinDeviceGroupAsync(device.DeviceId);

            // Load dữ liệu ban đầu song song
            await Task.WhenAll(
                LoadHistoryAsync(),
                LoadEnergyAsync(),
                LoadEventsAsync());
        }

        public async Task LeaveAsync()
        {
            if (_device is not null)
                await _signalR.LeaveDeviceGroupAsync(_device.DeviceId);
        }

        // ── Data loading ──────────────────────────────────────────────────
        private async Task LoadHistoryAsync()
        {
            if (_device is null) return;
            StatusText = "Đang tải lịch sử...";
            try
            {
                var list = await _api.GetTelemetryAsync(
                    _device.DeviceId,
                    DateTime.UtcNow.AddHours(-1),
                    DateTime.UtcNow);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    TempValues.Clear();
                    VibValues.Clear();
                    PowerValues.Clear();

                    // Lịch sử trả về SortByDescending nên đảo lại
                    foreach (var t in (list ?? []).AsEnumerable().Reverse().TakeLast(MaxPoints))
                    {
                        TempValues.Add(t.Metrics.Temperature);
                        VibValues.Add(t.Metrics.Vibration);
                        PowerValues.Add(t.Metrics.EstimatedPower);
                    }
                });
                StatusText = "Đã tải lịch sử 1 giờ qua";
            }
            catch (Exception ex)
            {
                StatusText = $"Lỗi: {ex.Message}";
            }
        }

        private async Task LoadEnergyAsync()
        {
            if (_device is null) return;
            var energy = await _api.GetEnergyAsync(_device.DeviceId, DateTime.UtcNow.Date);
            if (energy is null) return;
            EnergyKWh     = energy.EnergyKWh;
            EstimatedCost = energy.EstimatedCost;
        }

        private async Task LoadEventsAsync()
        {
            if (_device is null) return;
            var list = await _api.GetEventsAsync(_device.DeviceId);
            Application.Current.Dispatcher.Invoke(() =>
            {
                Events.Clear();
                foreach (var e in (list ?? []).OrderByDescending(x => x.Timestamp).Take(50))
                    Events.Add(e);
            });
        }

        private async Task SendCommandAsync(string command)
        {
            if (_device is null) return;
            StatusText = $"Đang gửi lệnh {command}...";
            var ok = await _api.SendCommandAsync(_device.DeviceId, new CommandRequest(command));
            StatusText = ok ? $"Đã gửi {command}" : $"Gửi {command} thất bại";
        }

        // ── Realtime handlers ─────────────────────────────────────────────
        private void OnTelemetryReceived(TelemetryDto dto)
        {
            if (_device is null || dto.DeviceId != _device.DeviceId) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Cập nhật giá trị hiện tại
                CurrentTemp      = dto.Metrics.Temperature;
                CurrentVibration = dto.Metrics.Vibration;
                CurrentPower     = dto.Metrics.EstimatedPower;
                RunState         = dto.Status.RunState;
                PowerState       = dto.Status.PowerState;
                WifiRssi         = dto.System.WifiRssi;
                Uptime           = dto.System.UpTime;

                // Thêm vào chart, giới hạn MaxPoints
                AddToChart(TempValues,  dto.Metrics.Temperature);
                AddToChart(VibValues,   dto.Metrics.Vibration);
                AddToChart(PowerValues, dto.Metrics.EstimatedPower);

                // Cập nhật điện năng ngay
                EnergyKWh     += (dto.Metrics.EstimatedPower / 1000.0) * (5.0 / 3600.0);
                EstimatedCost  = EnergyKWh * 3000;
            });
        }

        private void OnAlertReceived(AlertDto dto)
        {
            if (_device is null || dto.DeviceId != _device.DeviceId) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Alerts.Insert(0, new AlertItem
                {
                    DeviceId  = dto.DeviceId,
                    Type      = dto.Type,
                    Severity  = dto.Severity,
                    Value     = dto.Value,
                    Message   = dto.Message,
                    Timestamp = dto.Timestamp
                });

                // Giữ tối đa 20 alert
                while (Alerts.Count > 20)
                    Alerts.RemoveAt(Alerts.Count - 1);
            });
        }

        private static void AddToChart(ObservableCollection<double> col, double value)
        {
            col.Add(value);
            while (col.Count > 60)
                col.RemoveAt(0);
        }
    }
}

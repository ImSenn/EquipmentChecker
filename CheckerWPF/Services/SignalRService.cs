using Microsoft.AspNetCore.SignalR.Client;
using CheckerWPF.Models;

namespace CheckerWPF.Services
{
    public class SignalRService
    {
        private HubConnection? _connection;
        private readonly string _hubUrl;

        // Events để ViewModel subscribe
        public event Action<TelemetryDto>? TelemetryReceived;
        public event Action<AlertDto>? AlertReceived;
        public event Action<string>? ConnectionStateChanged;

        public bool IsConnected => _connection?.State == HubConnectionState.Connected;

        public SignalRService(string baseUrl)
        {
            _hubUrl = baseUrl.TrimEnd('/') + "/hubs/device";
        }

        public async Task ConnectAsync(string token)
        {
            if (_connection is not null)
                await _connection.DisposeAsync();

            _connection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, opts =>
                {
                    opts.AccessTokenProvider = () => Task.FromResult<string?>(token);
                })
                .WithAutomaticReconnect()
                .Build();

            // Đăng ký handlers
            _connection.On<TelemetryDto>("ReceiveTelemetry", dto =>
                TelemetryReceived?.Invoke(dto));

            _connection.On<AlertDto>("ReceiveAlert", dto =>
                AlertReceived?.Invoke(dto));

            _connection.Reconnecting += _ =>
            {
                ConnectionStateChanged?.Invoke("Reconnecting...");
                return Task.CompletedTask;
            };
            _connection.Reconnected += _ =>
            {
                ConnectionStateChanged?.Invoke("Connected");
                return Task.CompletedTask;
            };
            _connection.Closed += _ =>
            {
                ConnectionStateChanged?.Invoke("Disconnected");
                return Task.CompletedTask;
            };

            await _connection.StartAsync();
            ConnectionStateChanged?.Invoke("Connected");
        }

        // Join group của một thiết bị để nhận telemetry riêng
        public async Task JoinDeviceGroupAsync(string deviceId)
        {
            if (_connection is null || !IsConnected) return;
            await _connection.InvokeAsync("JoinDeviceGroup", deviceId);
        }

        public async Task LeaveDeviceGroupAsync(string deviceId)
        {
            if (_connection is null || !IsConnected) return;
            await _connection.InvokeAsync("LeaveDeviceGroup", deviceId);
        }

        public async Task DisconnectAsync()
        {
            if (_connection is not null)
                await _connection.DisposeAsync();
        }
    }
}

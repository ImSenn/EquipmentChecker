using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CheckerBA.Domain.Interfaces;
using CheckerBA.Domain.Entities;
using CheckerBA.Application.Services;

namespace CheckerBA.Infrastructure.Mqtt
{
    public class MqttListenerService : BackgroundService
    {
        private readonly IMqttService _mqttService;
        private readonly ILogger<MqttListenerService> _logger;
        private readonly TelemetryProcessingService _telemetryService;
        private readonly EventProcessingService _eventService;

        public MqttListenerService(
            IMqttService mqttService,
            ILogger<MqttListenerService> logger,
            TelemetryProcessingService telemetryService,
            EventProcessingService eventService)
        {
            _mqttService = mqttService;
            _logger = logger;
            _telemetryService = telemetryService;
            _eventService = eventService;
            _mqttService.MessageReceived += OnMessageReceived;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _mqttService.ConnectAsync("localhost", 1883, "CheckerBAListener");

            await _mqttService.SubscribeAsync("device/+/telemetry");
            await _mqttService.SubscribeAsync("device/+/event");
            await _mqttService.SubscribeAsync("device/+/ack");
            await _mqttService.SubscribeAsync("device/+/heartbeat");

            _logger.LogInformation("[MQTT] Listener khởi động thành công");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void OnMessageReceived(string topic, string payload)
        {
            // Chạy async trong background, không block thread MQTT
            _ = Task.Run(async () =>
            {
                try
                {
                    var parts = topic.Split('/');
                    if (parts.Length < 3) return;

                    var deviceId = parts[1];
                    var messageType = parts[2];

                    switch (messageType)
                    {
                        case "telemetry":
                            await HandleTelemetryAsync(deviceId, payload);
                            break;

                        case "event":
                            await HandleEventAsync(deviceId, payload);
                            break;

                        case "ack":
                            _logger.LogInformation("[ACK] {DeviceId}: {Payload}", deviceId, payload);
                            break;

                        case "heartbeat":
                            _logger.LogInformation("[HEARTBEAT] {DeviceId} online", deviceId);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // 1 message lỗi không làm crash cả service
                    _logger.LogError(ex, "[MQTT] Lỗi xử lý message từ topic: {Topic}", topic);
                }
            });
        }

        private async Task HandleTelemetryAsync(string deviceId, string payload)
        {
            // Parse JSON payload thành object Telemetry
            var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            var telemetry = new Telemetry
            {
                DeviceId = deviceId,
                Timestamp = root.GetProperty("timestamp").GetDateTime(),
                Metrics = new MetricsData
                {
                    Temperature = root.GetProperty("metrics").GetProperty("temperature").GetDouble(),
                    Vibration = root.GetProperty("metrics").GetProperty("vibration").GetDouble(),
                    estimatedPower = root.GetProperty("metrics").GetProperty("estimatedPower").GetDouble()
                },
                Status = new StatusData
                {
                    runState = root.GetProperty("status").GetProperty("runState").GetString()!,
                    powerState = root.GetProperty("status").GetProperty("powerState").GetString()!
                },
                System = new SystemData
                {
                    upTime = root.GetProperty("system").GetProperty("uptime").GetDouble(),
                    wifiRssi = root.GetProperty("system").GetProperty("wifiRssi").GetDouble(),
                    freeHeap = root.GetProperty("system").GetProperty("freeHeap").GetDouble()
                }
            };

            // Lưu vào MongoDB + tính điện năng
            await _telemetryService.ProcessTelemetryAsync(telemetry);
            _logger.LogInformation("[TELEMETRY] Đã lưu MongoDB - {DeviceId} | Nhiệt độ: {Temp}°C",
                deviceId, telemetry.Metrics.Temperature);
        }

        private async Task HandleEventAsync(string deviceId, string payload)
        {
            // Parse JSON payload thành object Event
            var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            var newEvent = new Event
            {
                DeviceId = deviceId,
                Timestamp = root.GetProperty("timestamp").GetDateTime(),
                events = new EventType
                {
                    Type = root.GetProperty("event").GetProperty("type").GetString()!,
                    severity = root.GetProperty("event").GetProperty("severity").GetString()!,
                    value = root.GetProperty("event").GetProperty("value").GetDouble(),
                    message = root.GetProperty("event").GetProperty("message").GetString()!
                }
            };

            // Lưu vào MongoDB
            await _eventService.ProcessEventAsync(newEvent);
            _logger.LogInformation("[EVENT] Đã lưu MongoDB - {DeviceId} | {Type} / {Severity}",
                deviceId, newEvent.events.Type, newEvent.events.severity);
        }
    }
}
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CheckerBA.Domain.Interfaces;

namespace CheckerBA.Infrastructure.Mqtt
{
    public class MqttListenerService : BackgroundService
    {
        private readonly IMqttService _mqttService;

        public MqttListenerService(IMqttService mqttService)
        {
            _mqttService = mqttService;
            _mqttService.MessageReceived += OnMessageReceived;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _mqttService.ConnectAsync("localhost", 1883, "CheckerBAListener");
            await _mqttService.SubscribeAsync("devices/+/telemetry");
            await _mqttService.SubscribeAsync("devices/+/energy");
            await _mqttService.SubscribeAsync("devices/+/heartbeat");
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void OnMessageReceived(string topic, string payload)
        {
            Console.WriteLine($"Received message on topic '{topic}': {payload}");
        }
    }
}
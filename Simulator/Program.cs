using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Protocol;

namespace Simulator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string deviceId = "MACHINE_01";
            var factory = new MqttClientFactory();
            var mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883)
                .WithClientId(deviceId)
                .WithCleanSession(true)
                .Build();

            await mqttClient.ConnectAsync(options);
            Console.WriteLine($"[SIMULATOR] Kết nối thành công: {deviceId}");

            var random = new Random();
            long uptime = 0;

            while (true)
            {
                uptime += 5;

                // Tạo dữ liệu telemetry giả lập
                var payload = new
                {
                    deviceId = deviceId,
                    timestamp = DateTime.UtcNow,
                    metrics = new
                    {
                        temperature = Math.Round(35.0 + random.NextDouble() * 45.0, 1),
                        vibration = Math.Round(random.NextDouble() * 0.05, 3),
                        estimatedPower = random.Next(450, 600)
                    },
                    status = new { runState = "RUNNING", powerState = "ON" },
                    system = new
                    {
                        uptime = uptime,
                        wifiRssi = random.Next(-80, -40),
                        freeHeap = random.Next(100000, 250000)
                    }
                };

                string json = JsonSerializer.Serialize(payload);
                string topic = $"device/{deviceId}/telemetry";

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(json)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                if (mqttClient.IsConnected)
                {
                    await mqttClient.PublishAsync(message);
                    Console.WriteLine($"[GỬI] {topic} | Nhiệt độ: {payload.metrics.temperature}°C | Power: {payload.metrics.estimatedPower}W");
                }

                // Chờ 5 giây rồi gửi tiếp
                await Task.Delay(5000);
            }
        }
    }
}
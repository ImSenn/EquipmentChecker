using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;

namespace Simulator
{
    class Program
    {
        // Raised whenever a message is received from the broker
        public static event Action<string, string>? MessageReceived;

        static async Task Main(string[] args)
        {
            string deviceId = "MACHINE_01";
            var factory = new MqttClientFactory();
            var mqttClient = factory.CreateMqttClient();

            mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                MessageReceived?.Invoke(topic, payload);
            };

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883)
                .WithClientId(deviceId)
                .WithCleanSession(true)
                .Build();

            await mqttClient.ConnectAsync(options);
            Console.WriteLine($"Connected as {deviceId}");

            // Keep the application running
            await Task.Delay(Timeout.Infinite);
        }
    }
}
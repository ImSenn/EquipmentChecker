using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using CheckerBA.Domain.Interfaces;

namespace CheckerBA.Infrastructure.Mqtt
{
    public class MqttClientService : IMqttService
    {
        private readonly IMqttClient _mqttClient;
        public event Action<string, string>? MessageReceived;

        public MqttClientService()
        {
            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                MessageReceived?.Invoke(topic, payload);
                return Task.CompletedTask;
            };
        }

        public async Task ConnectAsync(string broker, int port, string clientId)
        {
            var option = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithClientId(clientId)
                .WithCleanSession(true)
                .Build();

            if (!_mqttClient.IsConnected)
            {
                await _mqttClient.ConnectAsync(option);
            }
        }

        public async Task SubscribeAsync(string topic)
        {
            var subscribe = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(topic))
                .Build();

            await _mqttClient.SubscribeAsync(subscribe);
        }

        public async Task PublishAsync(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _mqttClient.PublishAsync(message);
        }
    }
}
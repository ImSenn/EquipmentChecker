using System.Threading.Tasks;

namespace CheckerBA.Domain.Interfaces
{
    public interface IMqttService
    {
        event Action<string, string>? MessageReceived;
        Task ConnectAsync(string broker, int port, string clientId);
        Task SubscribeAsync(string topic);
        Task PublishAsync(string topic, string payload);
    }
}
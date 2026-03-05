using System;
using System.Text.Json;
using System.Threading.Tasks;
using MQTTnet;

Console.WriteLine("Đang khởi động Simulator...");

string deviceId = "MACHINE_01";
var factory = new MqttClientFactory();
var mqttClient = factory.CreateMqttClient();

var options = factory.CreateClientOptionsBuilder()
    .WithTcpServer("localhost", 1883)
    .WithClientId($"{deviceId}_Simulator_{Guid.NewGuid().ToString().Substring(0, 5)}")
    .WithCleanSession(true)
    .Build();

try
{
    await mqttClient.ConnectAsync(options);
    Console.WriteLine("[SIMULATOR] KẾT NỐI MQTT THÀNH CÔNG! Đang bắt đầu gửi data...\n");
}
catch (Exception ex)
{
    Console.WriteLine($"[LỖI] Không thể kết nối. Hãy chắc chắn Mosquitto đang chạy. Lỗi: {ex.Message}");
    return;
}

var random = new Random();
long uptime = 0;

while (true)
{
    uptime += 5;

    // Nặn cục dữ liệu JSON
    var payloadObject = new
    {
        deviceId = deviceId,
        timestamp = DateTime.UtcNow,
        metrics = new
        {
            temperature = Math.Round(35.0 + (random.NextDouble() * 50.0), 1),
            vibration = Math.Round(random.NextDouble() * 5.0, 2),
            estimatedPower = random.Next(1400, 1600)
        },
        status = new { runState = "RUNNING", powerState = "ON" },
        system = new { uptime = uptime, wifiRssi = random.Next(-80, -40), freeHeap = random.Next(100000, 250000) }
    };

    string payloadJson = JsonSerializer.Serialize(payloadObject);
    string topic = $"device/{deviceId}/telemetry";

    var message = new MqttApplicationMessageBuilder()
        .WithTopic(topic)
        .WithPayload(payloadJson)
        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
        .Build();

    if (mqttClient.IsConnected)
    {
        await mqttClient.PublishAsync(message);
        Console.WriteLine($"[GỬI LÊN MQTT -> {topic}] Nhiệt độ: {payloadObject.metrics.temperature}°C");
    }

    await Task.Delay(5000);
}
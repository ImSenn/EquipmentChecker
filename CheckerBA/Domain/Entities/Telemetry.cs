using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CheckerBA.Domain.Entities
{
    public class Telemetry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string DeviceId { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public MetricsData Metrics { get; set; } = new();
        public StatusData Status { get; set; } = new();
        public SystemData System { get; set; } = new();
    }

    public class MetricsData
    {
        public double Temperature { get; set; }
        public double Vibration { get; set; }
        public double estimatedPower { get; set; }
    }

    public class StatusData
    {
        public string runState { get; set; } = null!;
        public string powerState { get; set; } = null!;
    }

    public class SystemData
    {
        public double upTime { get; set; }
        public double wifiRssi { get; set; }
        public double freeHeap { get; set; }
    }
}

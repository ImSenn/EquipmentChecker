using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CheckerBA.Domain.Entities
{
    public class Event
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string DeviceId { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public EventType events { get; set; } = new();
    }

    public class EventType
    {
        public string Type { get; set; } = null!;
        public string severity { get; set; } = null!;
        public double value { get; set; }
        public string message { get; set; } = null!;
    }
}

using System;
using MongoDB.Bson.Serialization.Attributes;

namespace CheckerBA.Domain.Entities
{
    public class Device
    {
        /// <summary>Primary key = deviceId (e.g. "MACHINE_01")</summary>
        [BsonId]
        public string DeviceId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public double PowerRating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

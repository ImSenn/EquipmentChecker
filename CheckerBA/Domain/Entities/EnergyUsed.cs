using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CheckerBA.Domain.Entities
{
    public class EnergyUsed
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string DeviceId { get; set; } = null!;
        public DateTime date { get; set; }
        public double energyKWh { get; set; }
        public double estimatedCost { get; set; }
    }
}
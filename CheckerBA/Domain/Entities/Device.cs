using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CheckerBA.Domain.Entities
{
    public class Device
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public double PowerRated { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

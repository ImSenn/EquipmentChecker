using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CheckerBA.Domain.Entities
{
    public class AppUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string Username { get; set; } = null!;

        /// <summary>BCrypt hash của password</summary>
        public string PasswordHash { get; set; } = null!;

        /// <summary>"Admin" hoặc "Operator"</summary>
        public string Role { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiWhatsAppVerification.Domain.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }             
        public string Username { get; set; }
        public string HashedPassword { get; set; } 
        public DateTime CreatedAt { get; set; }        
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}

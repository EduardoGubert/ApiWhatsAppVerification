using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ApiWhatsAppVerification.Domain.Entities
{
    public class PhoneNumberVerification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string PhoneNumber { get; set; }
        public bool HasWhatsApp { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}

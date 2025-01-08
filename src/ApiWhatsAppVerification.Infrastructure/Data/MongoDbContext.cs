using ApiWhatsAppVerification.Domain.Entities;
using MongoDB.Driver;

namespace ApiWhatsAppVerification.Infrastructure.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<PhoneNumberVerification> PhoneVerifications
            => _database.GetCollection<PhoneNumberVerification>("PhoneVerifications");

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    }
}

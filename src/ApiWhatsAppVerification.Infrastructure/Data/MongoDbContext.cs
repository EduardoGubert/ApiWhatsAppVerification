using ApiWhatsAppVerification.Domain.Entities;
using MongoDB.Driver;

namespace ApiWhatsAppVerification.Infrastructure.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            try
            {
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase(databaseName);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error connecting to MongoDB: {ex.Message}");
            }        }

        public IMongoCollection<PhoneNumberVerification> PhoneVerifications
            => _database.GetCollection<PhoneNumberVerification>("PhoneVerifications");

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    }
}

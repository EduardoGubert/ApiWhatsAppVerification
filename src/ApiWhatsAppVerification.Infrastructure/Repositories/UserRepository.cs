using ApiWhatsAppVerification.Application.Interfaces.Repositories;
using ApiWhatsAppVerification.Domain.Entities;
using ApiWhatsAppVerification.Infrastructure.Data;
using MongoDB.Driver;

namespace ApiWhatsAppVerification.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MongoDbContext _context;

        public UserRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Find(u => u.Username == username)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(User user)
        {
            await _context.Users.InsertOneAsync(user);
        }

        public async Task<bool> UpdateAsync(User user)
        {
            // Supondo que user.Id seja uma string representando ObjectId
            var filter = Builders<User>.Filter.Eq(x => x.Id, user.Id);

            // ReplaceOne substitui o documento inteiro pelo 'user' atualizado
            var result = await _context.Users.ReplaceOneAsync(filter, user);

            // Confirma se o Mongo reconheceu a operação e se ao menos 1 doc foi alterado
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, userId);

            var result = await _context.Users.DeleteOneAsync(filter);

            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

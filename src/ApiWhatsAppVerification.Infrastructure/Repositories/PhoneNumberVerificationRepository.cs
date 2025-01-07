using ApiWhatsAppVerification.Application.Interfaces.Repositories;
using ApiWhatsAppVerification.Domain.Entities;
using ApiWhatsAppVerification.Infrastructure.Data;
using MongoDB.Driver;

namespace ApiWhatsAppVerification.Infrastructure.Repositories
{
    public class PhoneNumberVerificationRepository : IPhoneNumberVerificationRepository
    {
        private readonly MongoDbContext _context;

        public PhoneNumberVerificationRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<PhoneNumberVerification> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.PhoneVerifications
                .Find(x => x.PhoneNumber == phoneNumber)
                .FirstOrDefaultAsync();
        }

        public async Task SaveAsync(PhoneNumberVerification entity)
        {
            await _context.PhoneVerifications.InsertOneAsync(entity);
        }
    }
}

using ApiWhatsAppVerification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWhatsAppVerification.Application.Interfaces.Repositories
{
    public interface IPhoneNumberVerificationRepository
    {
        Task<PhoneNumberVerification> GetByPhoneNumberAsync(string phoneNumber);
        Task SaveAsync(PhoneNumberVerification entity);
    }
}

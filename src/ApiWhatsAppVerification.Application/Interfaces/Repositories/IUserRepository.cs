using ApiWhatsAppVerification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWhatsAppVerification.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(string userId);
    }
}

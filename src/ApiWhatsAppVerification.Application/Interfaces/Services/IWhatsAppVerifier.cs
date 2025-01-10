using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWhatsAppVerification.Application.Interfaces.Services
{
    public interface IWhatsAppVerifier
    {
        Task<bool> VerifyWhithTwillioAsync(string phoneNumber);
        Task<bool> VerifyWhithWhatsAppCloudAsync(string phoneNumber);
    }
}

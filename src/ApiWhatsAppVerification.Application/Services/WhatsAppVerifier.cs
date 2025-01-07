using ApiWhatsAppVerification.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWhatsAppVerification.Application.Services
{
    public class WhatsAppVerifier : IWhatsAppVerifier
    {
        public Task<bool> VerifyAsync(string phoneNumber)
        {
            // Lógica real de chamada de API do WhatsApp, ou, por enquanto, simula
            return Task.FromResult(true); // Exemplo: simula sempre true
        }
    }
}

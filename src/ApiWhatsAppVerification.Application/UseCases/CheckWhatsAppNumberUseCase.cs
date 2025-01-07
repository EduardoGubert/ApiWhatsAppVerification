using ApiWhatsAppVerification.Application.Interfaces.Repositories;
using ApiWhatsAppVerification.Application.Interfaces.Services;
using ApiWhatsAppVerification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWhatsAppVerification.Application.UseCases
{
    public class CheckWhatsAppNumberUseCase
    {
        private readonly IPhoneNumberVerificationRepository _repository;
        private readonly IWhatsAppVerifier _whatsAppVerifier;

        public CheckWhatsAppNumberUseCase(IPhoneNumberVerificationRepository repository,
                                          IWhatsAppVerifier whatsAppVerifier)
        {
            _repository = repository;
            _whatsAppVerifier = whatsAppVerifier;
        }

        public async Task<PhoneNumberVerification> ExecuteAsync(string phoneNumber)
        {
            // Verifica se já existe
            var existingRecord = await _repository.GetByPhoneNumberAsync(phoneNumber);
            if (existingRecord != null)
            {
                return existingRecord;
            }

            // Chama serviço externo ou qualquer lógica para verificar se tem WhatsApp
            bool hasWhatsApp = await _whatsAppVerifier.VerifyAsync(phoneNumber);

            // Cria a entidade
            var verification = new PhoneNumberVerification
            {
                PhoneNumber = phoneNumber,
                HasWhatsApp = hasWhatsApp,
                VerifiedAt = DateTime.UtcNow
            };

            // Salva no repositório
            await _repository.SaveAsync(verification);

            return verification;
        }
    }
}

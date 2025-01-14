using ApiWhatsAppVerification.Application.Interfaces.Repositories;
using ApiWhatsAppVerification.Application.Interfaces.Services;
using ApiWhatsAppVerification.Domain.Entities;
using ApiWhatsAppVerification.Domain.Response;
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
        private readonly IEvolutionWhatsAppVerifier _evolutionWhatsAppVerifier;

        public CheckWhatsAppNumberUseCase(IPhoneNumberVerificationRepository repository,
                                          IWhatsAppVerifier whatsAppVerifier,
                                          IEvolutionWhatsAppVerifier evolutionWhatsAppVerifier)
        {
            _repository = repository;
            _whatsAppVerifier = whatsAppVerifier;
            _evolutionWhatsAppVerifier = evolutionWhatsAppVerifier;
        }

        public async Task<PhoneNumberVerification> ExecuteAsync(string phoneNumber)
        {
            // Verifica se já existe
            var existingRecord = await _repository.GetByPhoneNumberAsync(phoneNumber);
            if (existingRecord != null)
            {
                // Verifica se o número já foi confirmado como tendo WhatsApp
                if (existingRecord.HasWhatsApp)
                {
                    return existingRecord; // Retorna o registro existente se já está verificado e possui WhatsApp
                }

                // Verifica se a última verificação foi há mais de 60 minutos
                if (existingRecord.VerifiedAt.HasValue &&
                    (DateTime.UtcNow - existingRecord.VerifiedAt.Value).TotalMinutes < 1200)
                {
                    return existingRecord; // Retorna o registro existente se a última verificação foi há menos de 60 minutos
                }
            }

            // Chama serviço externo ou qualquer lógica para verificar se tem WhatsApp
            //bool hasWhatsApp = await _whatsAppVerifier.VerifyWhithTwillioAsync(phoneNumber);
            EvolutionNumberResponse hasWhatsApp = await _evolutionWhatsAppVerifier.VerifyWhatsAppNumber(phoneNumber);

            // Cria a entidade
            var verification = new PhoneNumberVerification
            {
                PhoneNumber = phoneNumber,
                HasWhatsApp = hasWhatsApp.Exists,
                VerifiedAt = DateTime.UtcNow
            };

            // Salva no repositório
            await _repository.SaveAsync(verification);

            return verification;
        }
    }
}

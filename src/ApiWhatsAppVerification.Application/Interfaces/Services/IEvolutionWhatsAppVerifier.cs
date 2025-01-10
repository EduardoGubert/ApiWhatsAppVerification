using ApiWhatsAppVerification.Domain.Response;
namespace ApiWhatsAppVerification.Application.Interfaces.Services
{
    public interface IEvolutionWhatsAppVerifier
    {
        Task<EvolutionNumberResponse> VerifyWhatsAppNumber(string phoneNumber);
    }
}

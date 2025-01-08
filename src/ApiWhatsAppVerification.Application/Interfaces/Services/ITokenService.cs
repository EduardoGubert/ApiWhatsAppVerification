namespace ApiWhatsAppVerification.Application.Interfaces.Services
{
    public interface ITokenService
    {
        public string GenerateJwtToken(string username);
    }
}

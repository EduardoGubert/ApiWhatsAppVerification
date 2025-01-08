using ApiWhatsAppVerification.Application.Interfaces.Repositories;
using ApiWhatsAppVerification.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;

namespace ApiWhatsAppVerification.Application.UseCases
{
    public class LoginUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public LoginUserUseCase(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<string> ExecuteAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                return null; // usuario não encontrado
            }

            // Verifica se o usuário está ativo
            if (!user.IsActive)
            {
                return null; // ou string indicando "Usuário inativo"
            }

            // Verifica a senha com Bcrypt
            if (!BCrypt.Net.BCrypt.Verify(password, user.HashedPassword))
            {
                return null; // senha inválida
            }

            // Gera o token JWT
            var token = _tokenService.GenerateJwtToken(username);
            return token;
        }
    }
}

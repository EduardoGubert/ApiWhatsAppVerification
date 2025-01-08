using ApiWhatsAppVerification.Application.Interfaces.Repositories;
using ApiWhatsAppVerification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWhatsAppVerification.Application.UseCases
{
    public class RegisterUserUseCase
    {
        private readonly IUserRepository _userRepository;

        public RegisterUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<string> ExecuteAsync(string username, string password, string email)
        {
            // Verifica se o usuário já existe
            var existingUser = await _userRepository.GetByUsernameAsync(username);
            if (existingUser != null)
            {
                return "Usuário já existe.";
            }

            // Faz o hash da senha
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // Cria a entidade de usuário
            var newUser = new User
            {
                Username = username,
                HashedPassword = hashedPassword,
                Email = email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Salva o novo usuário
            await _userRepository.CreateAsync(newUser);

            return "Usuário registrado com sucesso!";
        }
    }
}

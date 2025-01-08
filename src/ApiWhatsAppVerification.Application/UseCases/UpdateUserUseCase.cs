using ApiWhatsAppVerification.Application.Interfaces.Repositories;
using ApiWhatsAppVerification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWhatsAppVerification.Application.UseCases
{
    public class UpdateUserUseCase
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> ExecuteAsync(User userToUpdate)
        {
            // Regras de negócio ou validações, se necessário
            // Por exemplo: não permitir atualizar Username se for único, etc.

            var result = await _userRepository.UpdateAsync(userToUpdate);
            return result;
        }
    }
}

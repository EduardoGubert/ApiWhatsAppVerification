using ApiWhatsAppVerification.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWhatsAppVerification.Application.UseCases
{
    public class DeleteUserUseCase
    {
        private readonly IUserRepository _userRepository;

        public DeleteUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> ExecuteAsync(string userId)
        {
            // Se houver lógica extra (ex.: não deletar se o user for admin, etc.),
            // você faz aqui antes de chamar o repositório.

            return await _userRepository.DeleteAsync(userId);
        }
    }
}

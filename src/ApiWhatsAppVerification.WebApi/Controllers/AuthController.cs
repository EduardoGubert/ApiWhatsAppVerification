using ApiWhatsAppVerification.Application.UseCases;
using ApiWhatsAppVerification.Domain.Dtos;
using ApiWhatsAppVerification.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserUseCase _registerUserUseCase;
    private readonly UpdateUserUseCase _updateUserUseCase;
    private readonly DeleteUserUseCase _deleteUserUseCase;
    private readonly LoginUserUseCase _loginUserUseCase;

    public AuthController(
             RegisterUserUseCase registerUserUseCase,
             UpdateUserUseCase updateUserUseCase,
             DeleteUserUseCase deleteUserUseCase,
             LoginUserUseCase loginUserUseCase)
    {
        _registerUserUseCase = registerUserUseCase;
        _updateUserUseCase = updateUserUseCase;
        _deleteUserUseCase = deleteUserUseCase;
        _loginUserUseCase = loginUserUseCase;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Validação básica
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Username, Password e Email são obrigatórios.");
        }

        var resultMessage = await _registerUserUseCase.ExecuteAsync(
            request.Username,
            request.Password,
            request.Email);

        if (resultMessage == "Usuário já existe.")
        {
            return BadRequest(resultMessage);
        }

        return Ok(resultMessage);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromBody] User user)
    {
        var success = await _updateUserUseCase.ExecuteAsync(user);
        if (!success)
            return NotFound("Usuário não encontrado ou não atualizado.");

        return Ok("Usuário atualizado com sucesso!");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var success = await _deleteUserUseCase.ExecuteAsync(id);
        if (!success)
            return NotFound("Usuário não encontrado ou não excluído.");

        return Ok("Usuário deletado com sucesso!");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _loginUserUseCase.ExecuteAsync(
            request.Username,
            request.Password);

        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized("Credenciais inválidas.");
        }

        return Ok(new { token });
    }


    

}

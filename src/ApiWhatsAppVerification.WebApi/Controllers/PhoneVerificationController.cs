using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiWhatsAppVerification.Application.UseCases;
using ApiWhatsAppVerification.Domain.Entities;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Exige token JWT
public class PhoneVerificationController : ControllerBase
{
    private readonly CheckWhatsAppNumberUseCase _useCase;

    public PhoneVerificationController(CheckWhatsAppNumberUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpPost("check")]
    public async Task<IActionResult> Check([FromBody] string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return BadRequest("Phone number is required.");

        PhoneNumberVerification result = await _useCase.ExecuteAsync(phoneNumber);

        return Ok(new
        {
            phoneNumber = result.PhoneNumber,
            hasWhatsApp = result.HasWhatsApp,
            verifiedAt = result.VerifiedAt
        });
    }
}
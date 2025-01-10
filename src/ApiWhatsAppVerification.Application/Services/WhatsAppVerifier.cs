using ApiWhatsAppVerification.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Verify.V2.Service;
using Twilio.Types;

namespace ApiWhatsAppVerification.Application.Services
{
    public class WhatsAppVerifier : IWhatsAppVerifier
    {
        private readonly IConfiguration _configuration;
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _twilioWhatsAppNumber;
        private readonly string _verifyServiceSid;

        public WhatsAppVerifier(IConfiguration configuration)
        {
            _configuration = configuration;
            _accountSid = _configuration["Twilio:AccountSid"];
            _authToken = _configuration["Twilio:AuthToken"];
            _twilioWhatsAppNumber = _configuration["Twilio:WhatsAppFromNumber"];
            _verifyServiceSid = _configuration["Twilio:VerifyServiceSid"];
        }

        public async Task<bool> VerifyWhithTwillioAsync(string phoneNumber)
        { 
            TwilioClient.Init(_accountSid, _authToken);

            try
            {
                var message = await MessageResource.CreateAsync(
                    body: "Verificando se este número possui WhatsApp...",
                    from: new PhoneNumber($"whatsapp:{_twilioWhatsAppNumber}"),
                    to: new PhoneNumber($"whatsapp:{phoneNumber}")
                );

                if (message.ErrorCode.HasValue)
                    return false;
                

                // Se conseguiu enviar a mensagem, assume que é WhatsApp.
                // "Queued" ou "Sent" -> Significa que o número tem WA.
                return true;
            }
            catch (Twilio.Exceptions.ApiException ex)
            {
                // Se for um erro que indica que não há WhatsApp
                // ex.Status pode ser 400 / 404, ex.Code pode ser 63018 etc.
                // Log ex e retorne false
                return false;
            }
            catch (Exception)
            {
                // Se deu outro erro, decida como tratar.
                return false;
            }

        }

        public async Task<bool> SendWhatsAppVerification(string phoneNumber)
        {
            try
            {
                TwilioClient.Init(_accountSid, _authToken);

                var verification = await VerificationResource.CreateAsync(
                    to: phoneNumber,
                    channel: "whatsapp",
                    pathServiceSid: _verifyServiceSid
                );

                return verification.Status == "pending";
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CheckVerificationCode(string phoneNumber, string code)
        {
            try
            {
                TwilioClient.Init(_accountSid, _authToken);

                var verificationCheck = await VerificationCheckResource.CreateAsync(
                    to: phoneNumber,
                    code: code,
                    pathServiceSid: _verifyServiceSid
                );

                return verificationCheck.Status == "approved";
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> VerifyWhithWhatsAppCloudAsync(string phoneNumber)
        {
            return false;
            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Authorization =
            //    new AuthenticationHeaderValue("Bearer", yourCloudApiToken);

            //// Endpoint e payload para enviar mensagem:
            //var requestBody = new
            //{
            //    messaging_product = "whatsapp",
            //    to = phoneNumber,
            //    type = "text",
            //    text = new { preview_url = false, body = "Verificando se esse numero possui WA..." }
            //};
            //var json = JsonConvert.SerializeObject(requestBody);
            //var content = new StringContent(json, Encoding.UTF8, "application/json");

            //var response = await client.PostAsync("https://graph.facebook.com/v14.0/<yourPhoneId>/messages", content);

        }
    }
}

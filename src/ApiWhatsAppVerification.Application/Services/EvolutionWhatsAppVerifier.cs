using ApiWhatsAppVerification.Application.Interfaces.Services;
using ApiWhatsAppVerification.Domain.Response;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ApiWhatsAppVerification.Application.Services
{
    public class EvolutionWhatsAppVerifier : IEvolutionWhatsAppVerifier
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EvolutionWhatsAppVerifier> _logger;

        private SemaphoreSlim _throttler = new SemaphoreSlim(1);
        private DateTime _lastRequestTime = DateTime.MinValue;
        private const int MIN_DELAY_MS = 6000; // 6 segundos entre requisições
        private const int MAX_REQUESTS_PER_DAY = 1000;
        private int _dailyRequestCount = 0;
        private DateTime _dailyCounterReset = DateTime.UtcNow;

        public EvolutionWhatsAppVerifier(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<EvolutionWhatsAppVerifier> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // Configura o HttpClient com a API key
            _httpClient.DefaultRequestHeaders.Add("apikey", _configuration["EvolutionApi:ApiKey"]);
            _httpClient.BaseAddress = new Uri(_configuration["EvolutionApi:BaseUrl"]);
        }

        public async Task<EvolutionNumberResponse> VerifyWhatsAppNumber(string phoneNumber)
        {
            await ThrottleRequest();
            try
            {
                phoneNumber = FormatPhoneNumber(phoneNumber);
                var instanceName = _configuration["EvolutionApi:InstanceName"];

                _logger.LogInformation($"Verificando número: {phoneNumber} na instância: {instanceName}");

                // Criar o objeto de request
                var requestBody = new { numbers = new[] { phoneNumber } };

                // Fazer a requisição POST
                var response = await _httpClient.PostAsJsonAsync(
                 $"/chat/whatsappNumbers/{Uri.EscapeDataString(instanceName)}",
                requestBody);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Resposta da API: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = JsonDocument.Parse(responseContent);
                    var numberResult = jsonResponse.RootElement.EnumerateArray().FirstOrDefault();

                    if (numberResult.ValueKind != JsonValueKind.Undefined)
                    {
                        var exists = numberResult.GetProperty("exists").GetBoolean();
                        return new EvolutionNumberResponse
                        {
                            Success = true,
                            Exists = exists,
                            Message = exists ? "Número possui WhatsApp" : "Número não possui WhatsApp"
                        };
                    }
                }

                _logger.LogError($"Erro ao verificar número. Status: {response.StatusCode}, Resposta: {responseContent}");
                return new EvolutionNumberResponse
                {
                    Success = false,
                    Message = $"Erro ao verificar número. Status: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar número WhatsApp");
                return new EvolutionNumberResponse
                {
                    Success = false,
                    Message = $"Erro interno: {ex.Message}"
                };
            }
        }

        private string FormatInstanceName(string instanceName)
        {
            // Remove acentos e caracteres especiais
            string normalizedString = instanceName.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Substitui espaços por hífens e converte para minúsculas
            return stringBuilder.ToString()
                .ToLower()
                .Replace(" ", "-")
                .Replace("ç", "c");
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Remove caracteres não numéricos
            phoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Adiciona código do país se necessário
            if (!phoneNumber.StartsWith("55"))
            {
                phoneNumber = "55" + phoneNumber;
            }

            return phoneNumber;
        }


        private async Task ThrottleRequest()
        {
            await _throttler.WaitAsync();
            try
            {
                var elapsed = DateTime.UtcNow - _lastRequestTime;
                if (elapsed.TotalMilliseconds < MIN_DELAY_MS)
                {
                    await Task.Delay(MIN_DELAY_MS - (int)elapsed.TotalMilliseconds);
                }

                // Reset diário
                if (DateTime.UtcNow.Date > _dailyCounterReset.Date)
                {
                    _dailyRequestCount = 0;
                    _dailyCounterReset = DateTime.UtcNow;
                }

                // Verifica limite diário
                if (_dailyRequestCount >= MAX_REQUESTS_PER_DAY)
                {
                    throw new RateLimitExceededException("Limite diário de verificações atingido");
                }

                _dailyRequestCount++;
                _lastRequestTime = DateTime.UtcNow;
            }
            finally
            {
                _throttler.Release();
            }
        }

    }
}

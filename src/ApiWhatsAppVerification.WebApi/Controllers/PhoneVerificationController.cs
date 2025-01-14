using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiWhatsAppVerification.Application.UseCases;
using ApiWhatsAppVerification.Domain.Entities;
using System.Formats.Asn1;
using System.Text;
using CsvHelper.Configuration;
using CsvHelper;
using ApiWhatsAppVerification.Domain.Request;

[ApiController]
[Route("api/[controller]")]
//[Authorize] // Exige token JWT
public class PhoneVerificationController : ControllerBase
{
    private readonly CheckWhatsAppNumberUseCase _useCase;

    public PhoneVerificationController(CheckWhatsAppNumberUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet("check")]
    public async Task<IActionResult> Check(string phoneNumber)
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

    [HttpPost("check-bulk")]
    public async Task<IActionResult> CheckBulk([FromBody] List<string> phoneNumbers)
    {
        if (phoneNumbers == null || phoneNumbers.Count == 0)
            return BadRequest("A lista de números de telefone é obrigatória.");

        var results = new List<object>();

        foreach (var phoneNumber in phoneNumbers)
        {
            var result = await _useCase.ExecuteAsync(phoneNumber);

            results.Add(new
            {
                PhoneNumber = phoneNumber,
                HasWhatsApp = result.HasWhatsApp,
                VerifiedAt = result.VerifiedAt
            });
        }

        return Ok(results);
    }

    //[HttpPost("check-from-csv")]
    //public async Task<IActionResult> CheckFromCsv([FromForm] IFormFile file)
    //{
    //    if (file == null || file.Length == 0)
    //        return BadRequest("O arquivo CSV é obrigatório.");

    //    var outputLines = new List<string>();

    //    using (var stream = file.OpenReadStream())
    //    using (var reader = new StreamReader(stream))
    //    using (var csvReader = new CsvReader(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
    //    {
    //        var records = csvReader.GetRecords<dynamic>().ToList();

    //        foreach (var record in records)
    //        {
    //            var telefone1 = record.telefone_1?.ToString();
    //            var telefone2 = record.telefone_2?.ToString();
    //            var outrosTelefones = record.outrosTelefones?.ToString();

    //            var telefones = new List<string> { telefone1, telefone2 };
    //            if (!string.IsNullOrEmpty(outrosTelefones))
    //            {
    //                telefones.AddRange(outrosTelefones.Split(","));
    //            }

    //            var whatsVerificados = new List<string>();

    //            foreach (var telefone in telefones.Where(t => !string.IsNullOrWhiteSpace(t)))
    //            {
    //                var result = await _useCase.ExecuteAsync(telefone);
    //                if (result.HasWhatsApp)
    //                {
    //                    whatsVerificados.Add(telefone);
    //                }
    //            }

    //            var verifiedPhones = string.Join(",", whatsVerificados);
    //            var line = $"{string.Join(",", record)},\"{verifiedPhones}\"";
    //            outputLines.Add(line);
    //        }
    //    }

    //    var outputCsv = string.Join("\n", outputLines);
    //    var outputBytes = Encoding.UTF8.GetBytes(outputCsv);

    //    return File(outputBytes, "text/csv", "output_with_whatsapp.csv");
    //}

    [HttpPost("check-from-json")]
    public async Task<IActionResult> CheckFromJson([FromBody] List<CompanyRequest> jsonData)
    {
        if (jsonData == null || jsonData.Count == 0)
            return BadRequest("Os dados em JSON são obrigatórios.");

        var outputLines = new List<string>();

        // Adiciona o cabeçalho do CSV
        var header = "Cnpj,RazaoSocial,NomeFantasia,DataAbertura,SituacaoCadastral,DataSituacaoCadastral," +
                     "NaturezaJuridica,MatrizFilial,CnaePrincipal,CnaesSecundarios,Cep,Endereco,Bairro,Municipio," +
                     "Uf,Telefone1,Telefone2,Email,CapitalSocial,Porte,OpcaoSimples,OpcaoMei,Socios,FaixaFaturamento," +
                     "QuantidadeFuncionarios,OutrosTelefones,OutrosEmails,whatsAppVerification";
        outputLines.Add(header);

        int totalLines = jsonData.Count;
        int totalNumbers = jsonData.Sum(r => new List<string> { r.Telefone1, r.Telefone2 }.Concat((r.OutrosTelefones ?? "").Split(",")).Count(t => !string.IsNullOrWhiteSpace(t)));
        int processedLines = 0;
        int processedNumbers = 0;

        foreach (var record in jsonData)
        {
            var telefones = new List<string> { record.Telefone1, record.Telefone2 };

            if (!string.IsNullOrEmpty(record.OutrosTelefones))
            {
                var outrosTelefones = record.OutrosTelefones?.ToString();
                telefones.AddRange(outrosTelefones.Split(","));
            }

            var whatsVerificados = new List<string>();

            foreach (var telefone in telefones.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                var result = await _useCase.ExecuteAsync(telefone);

                // Adiciona o resultado da verificação para cada número
                if (result.HasWhatsApp)
                {
                    whatsVerificados.Add($"{telefone} -> sim");
                }
                else
                {
                    whatsVerificados.Add($"{telefone} -> não");
                }

                processedNumbers++;
                Console.WriteLine($"Números processados: {processedNumbers}/{totalNumbers}");
            }

            // Junta os números verificados em uma única string
            var verifiedPhones = string.Join(" | ", whatsVerificados);

            // Gera a linha do CSV
            var line = $"{record.Cnpj},{record.RazaoSocial},{record.NomeFantasia},{record.DataAbertura}," +
                       $"{record.SituacaoCadastral},{record.DataSituacaoCadastral},{record.NaturezaJuridica}," +
                       $"{record.MatrizFilial},{record.CnaePrincipal},{record.CnaesSecundarios},{record.Cep}," +
                       $"{record.Endereco},{record.Bairro},{record.Municipio},{record.Uf},{record.Telefone1}," +
                       $"{record.Telefone2},{record.Email},{record.CapitalSocial},{record.Porte},{record.OpcaoSimples}," +
                       $"{record.OpcaoMei},{record.Socios},{record.FaixaFaturamento},{record.QuantidadeFuncionarios}," +
                       $"{record.OutrosTelefones},{record.OutrosEmails},\"{verifiedPhones}\"";

            outputLines.Add(line);

            processedLines++;
            Console.WriteLine($"Linhas processadas: {processedLines}/{totalLines}");
        }

        // Junta todas as linhas do CSV
        var outputCsv = string.Join("\n", outputLines);
        var outputBytes = Encoding.UTF8.GetBytes(outputCsv);

        return File(outputBytes, "text/csv", "output_with_whatsapp_from_json.csv");
    }

}

using System.Text.Json.Serialization;

namespace ApiWhatsAppVerification.Domain.Request
{
    public class CompanyRequest
    {
        [JsonPropertyName("cnpj")]
        public long? Cnpj { get; set; }
        [JsonPropertyName("razaoSocial")]
        public string? RazaoSocial { get; set; }
        [JsonPropertyName("nomeFantasia")]
        public string? NomeFantasia { get; set; }
        [JsonPropertyName("dataAbertura")]
        public string? DataAbertura { get; set; }
        [JsonPropertyName("situacaoCadastral")]
        public string? SituacaoCadastral { get; set; }
        [JsonPropertyName("dataSituacaoCadastral")]
        public string? DataSituacaoCadastral { get; set; }
        [JsonPropertyName("naturezaJuridica")]
        public string? NaturezaJuridica { get; set; }
        [JsonPropertyName("matrizFilial")]
        public string? MatrizFilial { get; set; }
        [JsonPropertyName("cnaePrincipal")]
        public string? CnaePrincipal { get; set; }
        [JsonPropertyName("cnaesSecundarios")]
        public string? CnaesSecundarios { get; set; }
        [JsonPropertyName("cep")]
        public long? Cep { get; set; }
        [JsonPropertyName("endereco")]
        public string? Endereco { get; set; }
        [JsonPropertyName("bairro")]
        public string? Bairro { get; set; }
        [JsonPropertyName("municipio")]
        public string? Municipio { get; set; }
        [JsonPropertyName("uf")]
        public string? Uf { get; set; }
        [JsonPropertyName("telefone_1")]
        public string? Telefone1 { get; set; }
        [JsonPropertyName("telefone_2")]
        public string? Telefone2 { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("capitalSocial")]
        public decimal? CapitalSocial { get; set; }
        [JsonPropertyName("porte")]
        public string? Porte { get; set; }
        [JsonPropertyName("opcaoSimples")]
        public string? OpcaoSimples { get; set; }
        [JsonPropertyName("opcaoMei")]
        public string? OpcaoMei { get; set; }
        [JsonPropertyName("socios")]
        public string? Socios { get; set; }
        [JsonPropertyName("faixa_faturamento")]
        public string? FaixaFaturamento { get; set; }
        [JsonPropertyName("quantidadeFuncionarios")]
        public string? QuantidadeFuncionarios { get; set; }
        [JsonPropertyName("outrosTelefones")]
        public string? OutrosTelefones { get; set; }
        [JsonPropertyName("outrosEmails")]
        public string? OutrosEmails { get; set; }
    }

}

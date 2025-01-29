using ApiWhatsAppVerification.Application.Interfaces.Services;
using ApiWhatsAppVerification.Application.Services;
using ApiWhatsAppVerification.Infrastructure.Ioc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Leitura das variáveis de ambiente e configuração
var mongoDbUri = Environment.GetEnvironmentVariable("MONGODB_URI") ?? builder.Configuration.GetConnectionString("MongoDb");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? builder.Configuration["Jwt:SecretKey"];
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "https://whatsapp-verification-frontend.vercel.app";

// Logs para verificar a leitura das variáveis de ambiente
Console.WriteLine($"MongoDB URI: {mongoDbUri}");
Console.WriteLine($"JWT Issuer: {jwtIssuer}");
Console.WriteLine($"JWT Audience: {jwtAudience}");
Console.WriteLine($"JWT Secret Key (raw): {jwtSecretKey}");
Console.WriteLine($"JWT Secret Key Length (UTF-8 Bytes): {Encoding.UTF8.GetBytes(jwtSecretKey).Length}");

if (string.IsNullOrEmpty(jwtSecretKey))
{
    throw new Exception("JWT_SECRET_KEY is not set in environment variables or configuration.");
}

var keyBytes = Encoding.UTF8.GetBytes(jwtSecretKey);

if (keyBytes.Length < 32)
{
    throw new Exception($"JWT secret key must be at least 32 bytes long. Current length: {keyBytes.Length} bytes.");
}

// Logs para verificar a chave JWT
Console.WriteLine($"JWT Secret Key Length (UTF-8 Bytes): {keyBytes.Length}");
Console.WriteLine($"JWT Secret Key (raw): {jwtSecretKey}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", builder =>
    {
        builder
            .WithOrigins(frontendUrl)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Adiciona serviços ao contêiner.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiWhatsAppVerification", Version = "v1" });

    // Definição de segurança (tipo Bearer)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. \r\n\r\n " +
                      "Insira 'Bearer' [espaço] e então seu token no campo de texto abaixo.\r\n\r\n" +
                      "Exemplo: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Configura a exigência de segurança global para as operações
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

if (string.IsNullOrEmpty(mongoDbUri))
{
    throw new Exception("MongoDB connection string not found in environment variables or configuration.");
}

// Registra a camada de Infraestrutura (MongoDB, Repositórios, etc.)
builder.Services.AddInfrastructure(builder.Configuration, mongoDbUri);

// Configura autenticação via JWT
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

// Adiciona autorização
builder.Services.AddAuthorization();

// Habilita Controllers (MVC)
builder.Services.AddControllers();

builder.Services.AddHttpClient<IEvolutionWhatsAppVerifier, EvolutionWhatsAppVerifier>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiWhatsAppVerification v1");
        c.RoutePrefix = string.Empty; // Isso fará o Swagger UI aparecer na raiz
    });
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    app.UseCors("ProductionPolicy");
}

app.UseCors("AllowSpecificOrigin");

// Ativa autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");

app.Run();
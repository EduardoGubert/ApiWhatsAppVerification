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

builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", builder =>
    {
        builder
            .WithOrigins(
                "https://whatsapp-verification-frontend-1ad4iu4u6.vercel.app",
                "https://whatsapp-verification-frontend.vercel.app"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials() // Adicione isso se estiver usando cookies
            .WithExposedHeaders("*"); // Permite expor headers adicionais se necessário
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
app.UseCors("ProductionPolicy");

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
}

app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "https://whatsapp-verification-frontend.vercel.app");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
        context.Response.Headers.Add("Access-Control-Max-Age", "86400");
        context.Response.StatusCode = 200;
        return;
    }

    await next();
});

// Ativa autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");

app.Run();
using ApiWhatsAppVerification.Application.Interfaces.Services;
using ApiWhatsAppVerification.Application.Services;
using ApiWhatsAppVerification.Infrastructure.Ioc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Text;
using ApiWhatsAppVerification.Application.UseCases;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var startupLogger = LoggerFactory.Create(config =>
{
    config.AddConsole();
    config.AddDebug();
    config.SetMinimumLevel(LogLevel.Information);
}).CreateLogger("Startup");

// Leitura das variáveis de ambiente e configuração
var mongoDbUri = Environment.GetEnvironmentVariable("MONGODB_URI") ?? builder.Configuration.GetConnectionString("MongoDb");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? builder.Configuration["Jwt:SecretKey"];
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? builder.Configuration["FronEndAUrl:FronEndAUrl"];
var evolutionApiUrl = Environment.GetEnvironmentVariable("EVOLUTION_API_URL")
    ?? builder.Configuration["EvolutionApi:BaseUrl"]
    ?? throw new InvalidOperationException("EVOLUTION_API_URL não configurada");
var evolutionApiKey = Environment.GetEnvironmentVariable("EVOLUTION_API_KEY")
    ?? builder.Configuration["EvolutionApi:ApiKey"]
    ?? throw new InvalidOperationException("EVOLUTION_API_KEY não configurada");

if (string.IsNullOrEmpty(evolutionApiUrl) || string.IsNullOrEmpty(evolutionApiKey))
{
    startupLogger.LogError("Configurações da Evolution API não encontradas nas variáveis de ambiente");
    throw new InvalidOperationException("Configurações da Evolution API não encontradas");
}

startupLogger.LogInformation($"Evolution API URL configurada: {evolutionApiUrl}");
builder.Configuration["EvolutionApiUrl"] = evolutionApiUrl;
builder.Configuration["EvolutionApiKey"] = evolutionApiKey;


var keyBytes = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", builder =>
    {
        builder
            .WithOrigins("https://whatsapp-verification-frontend.vercel.app")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("Authorization")
            .SetIsOriginAllowed(origin => true);
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiWhatsAppVerification", Version = "v1" });

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

// Configure HttpClient for EvolutionWhatsAppVerifier
builder.Services.AddHttpClient("EvolutionApi", client =>
{
    client.BaseAddress = new Uri(evolutionApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("apikey", evolutionApiKey);
});

// Registra a camada de Infraestrutura
builder.Services.AddInfrastructure(builder.Configuration, mongoDbUri);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                logger.LogError($"Falha na autenticação: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                logger.LogInformation("Token validado com sucesso!");
                logger.LogInformation($"Claims: {string.Join(", ", context.Principal.Claims.Select(c => $"{c.Type}: {c.Value}"))}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                logger.LogWarning($"Challenge: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// Middleware de diagnóstico
app.MapGet("/diagnostic", async (HttpContext context) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    try
    {
        using var scope = context.RequestServices.CreateScope();
        var services = new
        {
            HasLogger = scope.ServiceProvider.GetService<ILogger<PhoneVerificationController>>() != null,
            HasVerifier = scope.ServiceProvider.GetService<IEvolutionWhatsAppVerifier>() != null,
            HasUseCase = scope.ServiceProvider.GetService<CheckWhatsAppNumberUseCase>() != null,
            Environment = app.Environment.EnvironmentName,
            HasMongoDb = !string.IsNullOrEmpty(mongoDbUri),
            HasEvolutionApiUrl = !string.IsNullOrEmpty(evolutionApiUrl),
            EvolutionApiUrl = evolutionApiUrl // Apenas para debug, remova em produção
        };
        return Results.Ok(services);
    }
    catch (Exception ex)
    {
        logger.LogError($"Erro no diagnóstico: {ex.Message}");
        return Results.StatusCode(500);
    }
});

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiWhatsAppVerification v1");
        c.RoutePrefix = string.Empty;
    });
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("ProductionPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "https://whatsapp-verification-frontend.vercel.app");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        context.Response.Headers.Add("Access-Control-Max-Age", "86400");
        context.Response.StatusCode = 200;
        return;
    }

    await next();
});

app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
using ApiWhatsAppVerification.Application.Interfaces.Repositories;
using ApiWhatsAppVerification.Application.Interfaces.Services;
using ApiWhatsAppVerification.Application.Services;
using ApiWhatsAppVerification.Application.UseCases;
using ApiWhatsAppVerification.Infrastructure.Data;
using ApiWhatsAppVerification.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace ApiWhatsAppVerification.Infrastructure.Ioc
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
                                                           IConfiguration configuration, string mongoDbUri = null,string mongoDbName = null)
        {
            var logger = LoggerFactory.Create(config => config.AddConsole())
                                    .CreateLogger("Infrastructure");

            logger.LogInformation("Configurando infraestrutura...");

            // Configura Mongo
            var connectionString = Environment.GetEnvironmentVariable("MONGODB_URI") ??
            configuration.GetConnectionString("MongoDb");

            logger.LogInformation("MongoDB: " + connectionString);

            var databaseName = Environment.GetEnvironmentVariable("DATABASENAME") ?? 
                configuration["DatabaseName"];

            services.AddSingleton(new MongoDbContext(connectionString, databaseName));

            // Configurações
            services.Configure<Dictionary<string, string>>(configuration.GetSection("AppSettings"));

            // HttpClient nomeado para Evolution API
            services.AddHttpClient("EvolutionApi", client =>
            {
                var baseUrl = Environment.GetEnvironmentVariable("EVOLUTION_API_URL") ??
                                configuration["EvolutionApiUrl"];

                logger.LogInformation("BaseURLEvolutionAPI: " +  baseUrl);

                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new InvalidOperationException("EvolutionApiUrl não está configurado");
                }
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // Repositórios
            services.AddScoped<IPhoneNumberVerificationRepository, PhoneNumberVerificationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Serviços
            services.AddScoped<IEvolutionWhatsAppVerifier>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("EvolutionApi");
                var config = sp.GetRequiredService<IConfiguration>();
                var logger = sp.GetRequiredService<ILogger<EvolutionWhatsAppVerifier>>();
                var rotator = sp.GetRequiredService<InstanceRotatorUseCase>();
                return new EvolutionWhatsAppVerifier(httpClient, config, logger, rotator);
            });
            services.AddScoped<IWhatsAppVerifier, WhatsAppVerifier>();
            services.AddScoped<ITokenService, TokenService>();

            // UseCases
            services.AddSingleton<InstanceRotatorUseCase>();
            services.AddScoped<CheckWhatsAppNumberUseCase>();
            services.AddScoped<RegisterUserUseCase>();
            services.AddScoped<UpdateUserUseCase>();
            services.AddScoped<DeleteUserUseCase>();          
            services.AddScoped<LoginUserUseCase>();

            return services;
        }
    }
}

using ApiWhatsAppVerification.Application.Interfaces.Repositories;
using ApiWhatsAppVerification.Application.Interfaces.Services;
using ApiWhatsAppVerification.Application.Services;
using ApiWhatsAppVerification.Application.UseCases;
using ApiWhatsAppVerification.Infrastructure.Data;
using ApiWhatsAppVerification.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;


namespace ApiWhatsAppVerification.Infrastructure.Ioc
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
                                                           IConfiguration configuration)
        {
            // Configura Mongo
            var connectionString = configuration.GetConnectionString("MongoDb");
            var databaseName = configuration.GetValue<string>("DatabaseName");
            services.AddSingleton(new MongoDbContext(connectionString, databaseName));

            // Repositórios
            services.AddScoped<IPhoneNumberVerificationRepository, PhoneNumberVerificationRepository>();

            // Serviços
            services.AddScoped<IWhatsAppVerifier, WhatsAppVerifier>();

            // UseCases
            services.AddScoped<CheckWhatsAppNumberUseCase>();

            return services;
        }
    }
}

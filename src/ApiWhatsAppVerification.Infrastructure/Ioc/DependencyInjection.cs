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
            services.AddScoped<IUserRepository, UserRepository>();

            // Serviços
            services.AddScoped<IWhatsAppVerifier, WhatsAppVerifier>();
            services.AddScoped<ITokenService, TokenService>();

            // UseCases
            services.AddScoped<CheckWhatsAppNumberUseCase>();
            services.AddScoped<RegisterUserUseCase>();
            services.AddScoped<UpdateUserUseCase>();
            services.AddScoped<DeleteUserUseCase>();          
            services.AddScoped<LoginUserUseCase>();

            return services;
        }
    }
}

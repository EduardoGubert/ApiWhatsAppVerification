using ApiWhatsAppVerification.Infrastructure.Ioc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiWhatsAppVerification", Version = "v1" });

    // 1) Definição de segurança (tipo Bearer)
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

    // 2) Configura a exigência de segurança global para as operações
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



// 1.1) Registra a camada de Infraestrutura (MongoDB, Reposit�rios, etc.)
builder.Services.AddInfrastructure(builder.Configuration);


// 1.2) Configura autentica��o via JWT
builder.Services
    .AddAuthentication(options =>
    {
        // Define o esquema de autentica��o e desafio como JwtBearer
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Em ambiente de produ��o, mantenha RequireHttpsMetadata como true
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        // Par�metros de valida��o do token
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

// 1.3) Adiciona autoriza��o (necess�rio depois de AddAuthentication)
builder.Services.AddAuthorization();

// 1.4) Habilita Controllers (MVC)
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// 3.1) Ativa autentica��o e autoriza��o
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseHttpsRedirection();

app.Run();


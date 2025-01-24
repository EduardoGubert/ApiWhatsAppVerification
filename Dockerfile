# -------------------------
# 1) Etapa de Build
# -------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copia todos os .csproj
COPY src/ApiWhatsAppVerification.WebApi/ApiWhatsAppVerification.WebApi.csproj src/ApiWhatsAppVerification.WebApi/
COPY src/ApiWhatsAppVerification.Application/ApiWhatsAppVerification.Application.csproj src/ApiWhatsAppVerification.Application/
COPY src/ApiWhatsAppVerification.Infrastructure/ApiWhatsAppVerification.Infrastructure.csproj src/ApiWhatsAppVerification.Infrastructure/
COPY src/ApiWhatsAppVerification.Domain/ApiWhatsAppVerification.Domain.csproj src/ApiWhatsAppVerification.Domain/

# Restaura as dependências
RUN dotnet restore src/ApiWhatsAppVerification.WebApi/ApiWhatsAppVerification.WebApi.csproj

# Copia todo o código
COPY . .

# Publica o projeto
RUN dotnet publish src/ApiWhatsAppVerification.WebApi/ApiWhatsAppVerification.WebApi.csproj -c Release -o out

# -------------------------
# 2) Etapa de Runtime
# -------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Copia os arquivos publicados
COPY --from=build /app/out ./

# Variáveis de ambiente para produção
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:$PORT

# O Render vai injetar a variável PORT
EXPOSE $PORT

# Executa a aplicação
ENTRYPOINT ["dotnet", "ApiWhatsAppVerification.WebApi.dll"]
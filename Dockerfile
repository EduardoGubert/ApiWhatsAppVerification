# -------------------------
# 1) Etapa de Build
# -------------------------
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

    # Cria a pasta /app no container
    WORKDIR /app
    
    # Copia todos os .csproj de cada projeto para as respectivas pastas
    COPY src/ApiWhatsAppVerification.WebApi/ApiWhatsAppVerification.WebApi.csproj src/ApiWhatsAppVerification.WebApi/
    COPY src/ApiWhatsAppVerification.Application/ApiWhatsAppVerification.Application.csproj src/ApiWhatsAppVerification.Application/
    COPY src/ApiWhatsAppVerification.Infrastructure/ApiWhatsAppVerification.Infrastructure.csproj src/ApiWhatsAppVerification.Infrastructure/
    COPY src/ApiWhatsAppVerification.Domain/ApiWhatsAppVerification.Domain.csproj src/ApiWhatsAppVerification.Domain/
    
    # Restaura as dependências a partir do projeto principal (WebApi)
    RUN dotnet restore src/ApiWhatsAppVerification.WebApi/ApiWhatsAppVerification.WebApi.csproj
    
    # Agora copia TODO o restante do código-fonte (não só .csproj)
    COPY . .
    
    # Publica o projeto principal em modo Release
    RUN dotnet publish src/ApiWhatsAppVerification.WebApi/ApiWhatsAppVerification.WebApi.csproj -c Release -o out
    
    # -------------------------
    # 2) Etapa de Runtime
    # -------------------------
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
    
    # Cria a pasta /app no container para rodar a aplicação
    WORKDIR /app
    
    # Copia os arquivos publicados na etapa de Build
    COPY --from=build /app/out ./
    
    # Expor a porta 80 (opcional, mas recomendado)
    EXPOSE 8080
    ENV ASPNETCORE_URLS=http://+:8080
    
    # Executa a aplicação
    ENTRYPOINT ["dotnet", "ApiWhatsAppVerification.WebApi.dll"]
    
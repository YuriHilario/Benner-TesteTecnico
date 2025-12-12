# ====================================================================================
# STAGE 1: BASE (Execution Environment)
# ====================================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Usa um usuário não-root para execução (boa prática de segurança)
USER $APP_UID

WORKDIR /app
EXPOSE 8080
EXPOSE 8081



# ====================================================================================
# STAGE 2: BUILD
# ====================================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# 1. Copia os arquivos de projetos (multi-camadas)
COPY ["MicroOndas.Web/MicroOndas.Web.csproj", "MicroOndas.Web/"]
COPY ["MicroOndas.Application/MicroOndas.Application.csproj", "MicroOndas.Application/"]
COPY ["MicroOndas.Domain/MicroOndas.Domain.csproj", "MicroOndas.Domain/"]

# 2. Restaura dependências
RUN dotnet restore "MicroOndas.Web/MicroOndas.Web.csproj"

# 3. Copia todo o código restante
COPY . .

# 4. INSTALAÇÃO DO LIBMAN (para restaurar signalr.js ou outros assets)
WORKDIR "/src/MicroOndas.Web"

# Instala ferramenta global
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli

# Ajusta o PATH para permitir rodar "libman" diretamente
ENV PATH="$PATH:/root/.dotnet/tools"

# Copia libman.json da aplicação
COPY ["MicroOndas.Web/libman.json", "libman.json"]

# Restaura os arquivos do LibMan
RUN libman restore

# 5. Build da aplicação
RUN dotnet build "MicroOndas.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build



# ====================================================================================
# STAGE 3: PUBLISH
# ====================================================================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "MicroOndas.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false



# ====================================================================================
# STAGE 4: FINAL (Production Image)
# ====================================================================================
FROM base AS final

WORKDIR /app

# Copia da stage publish
COPY --from=publish /app/publish .

# Executa a aplicação
ENTRYPOINT ["dotnet", "MicroOndas.Web.dll"]

# ====================================================================================
# STAGE 1: BASE (Execution Environment)
# ====================================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app
EXPOSE 8080
EXPOSE 8081



# ====================================================================================
# STAGE 2: BUILD
# ====================================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# 1. Copia os arquivos de projetos (TODAS as camadas)
COPY ["MicroOndas.Web/MicroOndas.Web.csproj", "MicroOndas.Web/"]
COPY ["MicroOndas.Application/MicroOndas.Application.csproj", "MicroOndas.Application/"]
COPY ["MicroOndas.Domain/MicroOndas.Domain.csproj", "MicroOndas.Domain/"]
COPY ["MicroOndas.Infrastructure/MicroOndas.Infrastructure.csproj", "MicroOndas.Infrastructure/"]

# 2. Restaura dependências
RUN dotnet restore "MicroOndas.Web/MicroOndas.Web.csproj"

# 3. Copia todo o código restante
COPY . .

# 4. LibMan (SignalR / assets)
WORKDIR "/src/MicroOndas.Web"

RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
ENV PATH="$PATH:/root/.dotnet/tools"

RUN libman restore

# 5. Build
RUN dotnet build "MicroOndas.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build



# ====================================================================================
# STAGE 3: PUBLISH
# ====================================================================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "MicroOndas.Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false



# ====================================================================================
# STAGE 4: FINAL
# ====================================================================================
FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MicroOndas.Web.dll"]

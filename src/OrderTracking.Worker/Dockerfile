FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar arquivos de projeto
COPY ["src/OrderTracking.Worker/OrderTracking.Worker.csproj", "src/OrderTracking.Worker/"]
COPY ["src/OrderTracking.Application/OrderTracking.Application.csproj", "src/OrderTracking.Application/"]
COPY ["src/OrderTracking.Domain/OrderTracking.Domain.csproj", "src/OrderTracking.Domain/"]
COPY ["src/OrderTracking.Infrastructure/OrderTracking.Infrastructure.csproj", "src/OrderTracking.Infrastructure/"]
COPY ["src/OrderTracking.Shared/OrderTracking.Shared.csproj", "src/OrderTracking.Shared/"]

# Restaurar dependências
RUN dotnet restore "src/OrderTracking.Worker/OrderTracking.Worker.csproj"

# Copiar código fonte
COPY . .

# Build
WORKDIR "/src/src/OrderTracking.Worker"
RUN dotnet build "OrderTracking.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OrderTracking.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderTracking.Worker.dll"]
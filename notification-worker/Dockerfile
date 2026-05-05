# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copiar app compilada
COPY --from=build /app/publish .

# Ejecutar
ENTRYPOINT ["dotnet", "notification-worker.dll"]

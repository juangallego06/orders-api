# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Puerto 3000
ENV ASPNETCORE_URLS=http://+:3000

# Copiar app compilada
COPY --from=build /app/publish .

# Ejecutar
ENTRYPOINT ["dotnet", "Orders.API.dll"]
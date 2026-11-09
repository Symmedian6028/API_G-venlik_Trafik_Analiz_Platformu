# 1. Derleme (Build) Aşaması (.NET 10 SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Bağımlılıkların önbelleğe alınması için projeleri kopyala ve restore et
COPY ["ApiSecurityPlatform/ApiSecurityPlatform.csproj", "ApiSecurityPlatform/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

RUN dotnet restore "ApiSecurityPlatform/ApiSecurityPlatform.csproj"

# Tüm kaynak kodları kopyala ve derle
COPY . .
WORKDIR "/src/ApiSecurityPlatform"
RUN dotnet publish "ApiSecurityPlatform.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Çalıştırma (Runtime) Aşaması (.NET 10 ASP.NET)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5253
EXPOSE 5253

ENTRYPOINT ["dotnet", "ApiSecurityPlatform.dll"]
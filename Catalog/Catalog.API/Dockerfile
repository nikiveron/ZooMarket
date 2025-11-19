FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы проектов Catalog
COPY ["Catalog/Catalog.API/Catalog.API.csproj", "Catalog/Catalog.API/"]
COPY ["Catalog/Catalog.Application/Catalog.Application.csproj", "Catalog/Catalog.Application/"]
COPY ["Catalog/Catalog.Domain/Catalog.Domain.csproj", "Catalog/Catalog.Domain/"]
COPY ["Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "Catalog/Catalog.Infrastructure/"]

# Восстанавливаем зависимости Catalog
RUN dotnet restore "Catalog/Catalog.API/Catalog.API.csproj"

# Копируем весь исходный код
COPY . .

WORKDIR "/src/Catalog/Catalog.API"
RUN dotnet build "Catalog.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Catalog.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
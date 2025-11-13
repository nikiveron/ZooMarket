FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы проектов Order
COPY ["Order/Order.API/Order.API.csproj", "Order/Order.API/"]
COPY ["Order/Order.Application/Order.Application.csproj", "Order/Order.Application/"]
COPY ["Order/Order.Domain/Order.Domain.csproj", "Order/Order.Domain/"]
COPY ["Order/Order.Infrastructure/Order.Infrastructure.csproj", "Order/Order.Infrastructure/"]

# Восстанавливаем зависимости Order
RUN dotnet restore "Order/Order.API/Order.API.csproj"

# Копируем весь исходный код
COPY . .

WORKDIR "/src/Order/Order.API"
RUN dotnet build "Order.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Order.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.API.dll"]
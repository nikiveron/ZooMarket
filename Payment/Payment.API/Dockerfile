FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы проектов Payment
COPY ["Payment/Payment.API/Payment.API.csproj", "Payment/Payment.API/"]
COPY ["Payment/Payment.Application/Payment.Application.csproj", "Payment/Payment.Application/"]
COPY ["Payment/Payment.Domain/Payment.Domain.csproj", "Payment/Payment.Domain/"]
COPY ["Payment/Payment.Infrastructure/Payment.Infrastructure.csproj", "Payment/Payment.Infrastructure/"]

# Восстанавливаем зависимости Payment
RUN dotnet restore "Payment/Payment.API/Payment.API.csproj"

# Копируем весь исходный код
COPY . .

WORKDIR "/src/Payment/Payment.API"
RUN dotnet build "Payment.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Payment.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payment.API.dll"]
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы проектов Review
COPY ["Review/Review.API/Review.API.csproj", "Review/Review.API/"]
COPY ["Review/Review.Application/Review.Application.csproj", "Review/Review.Application/"]
COPY ["Review/Review.Domain/Review.Domain.csproj", "Review/Review.Domain/"]
COPY ["Review/Review.Infrastructure/Review.Infrastructure.csproj", "Review/Review.Infrastructure/"]

# Восстанавливаем зависимости Review
RUN dotnet restore "Review/Review.API/Review.API.csproj"

# Копируем весь исходный код
COPY . .

WORKDIR "/src/Review/Review.API"
RUN dotnet build "Review.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Review.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Review.API.dll"]
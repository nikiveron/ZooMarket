FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы проектов User
COPY ["User/User.API/User.API.csproj", "User/User.API/"]
COPY ["User/User.Application/User.Application.csproj", "User/User.Application/"]
COPY ["User/User.Domain/User.Domain.csproj", "User/User.Domain/"]
COPY ["User/User.Infrastructure/User.Infrastructure.csproj", "User/User.Infrastructure/"]

# Восстанавливаем зависимости User
RUN dotnet restore "User/User.API/User.API.csproj"

# Копируем весь исходный код
COPY . .

WORKDIR "/src/User/User.API"
RUN dotnet build "User.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "User.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "User.API.dll"]
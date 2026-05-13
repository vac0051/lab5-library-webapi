# Укажите этот Dockerfile в корне проекта Library.Api (внутри папки Lab5.LibraryApi\Library.Api)

# Базовый образ для выполнения приложения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Базовый образ для сборки (содержит SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем файлы проектов для восстановления зависимостей
# В Visual Studio контекстом сборки является папка решения (.sln)
COPY ["Library.Api/Library.Api.csproj", "Library.Api/"]
COPY ["Library.Data/Library.Data.csproj", "Library.Data/"]
RUN dotnet restore "./Library.Api/Library.Api.csproj"

# Копируем весь остальной код и собираем проект
COPY . .
WORKDIR "/src/Library.Api"
RUN dotnet build "./Library.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этап публикации
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Library.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Задаем порт 80 для приложения (по умолчанию в .NET 8 это 8080)
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "Library.Api.dll"]

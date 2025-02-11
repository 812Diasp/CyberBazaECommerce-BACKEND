FROM mcr.microsoft.com/dotnet/aspnet:9.0-nanoserver-1809 AS base
WORKDIR /app
EXPOSE 5248
EXPOSE 8081


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:9.0-nanoserver-1809 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CyberBazaECommerce/CyberBazaECommerce.csproj", "CyberBazaECommerce/"]
RUN dotnet restore "./CyberBazaECommerce/CyberBazaECommerce.csproj"
COPY . .
WORKDIR "/src/CyberBazaECommerce"
RUN dotnet build "./CyberBazaECommerce.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CyberBazaECommerce.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CyberBazaECommerce.dll"]
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SpaghettiManager.Api/SpaghettiManager.Api.csproj", "SpaghettiManager.Api/"]
RUN dotnet restore "SpaghettiManager.Api/SpaghettiManager.Api.csproj"
COPY . .
WORKDIR "/src/SpaghettiManager.Api"
RUN dotnet build "./SpaghettiManager.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SpaghettiManager.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpaghettiManager.Api.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["MicroService.ConsolidadoDiario/MicroService.ConsolidadoDiario.csproj", "MicroService.ConsolidadoDiario/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Data.MongoDb/Data.MongoDb.csproj", "Data.MongoDb/"]

RUN dotnet restore "MicroService.ConsolidadoDiario/MicroService.ConsolidadoDiario.csproj"

COPY . .

WORKDIR "/src/MicroService.ConsolidadoDiario"
RUN dotnet build "MicroService.ConsolidadoDiario.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "MicroService.ConsolidadoDiario.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MicroService.ConsolidadoDiario.dll"]
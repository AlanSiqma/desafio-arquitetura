FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base

USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Consumer.ConsolidadoDiario/Consumer.ConsolidadoDiario.csproj", "Consumer.ConsolidadoDiario/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Data.MongoDb/Data.MongoDb.csproj", "Data.MongoDb/"]

RUN dotnet restore "Consumer.ConsolidadoDiario/Consumer.ConsolidadoDiario.csproj"

COPY . .

WORKDIR "/src/Consumer.ConsolidadoDiario"

RUN dotnet build "Consumer.ConsolidadoDiario.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Consumer.ConsolidadoDiario.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Consumer.ConsolidadoDiario.dll"]
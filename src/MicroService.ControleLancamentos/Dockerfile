 FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

USER $APP_UID
WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["MicroService.ControleLancamentos/MicroService.ControleLancamentos.csproj", "MicroService.ControleLancamentos/"]
COPY ["Domain/Domain.csproj", "Domain/"]

RUN dotnet restore "MicroService.ControleLancamentos/MicroService.ControleLancamentos.csproj"

COPY . .

WORKDIR "/src/MicroService.ControleLancamentos"

RUN dotnet build "MicroService.ControleLancamentos.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "MicroService.ControleLancamentos.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MicroService.ControleLancamentos.dll"]
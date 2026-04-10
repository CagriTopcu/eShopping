FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ServiceDefaults/ServiceDefaults.csproj src/ServiceDefaults/
COPY src/Shared/Shared.BuildingBlocks/Shared.BuildingBlocks.csproj src/Shared/Shared.BuildingBlocks/
COPY src/Shared/Shared.Contracts/Shared.Contracts.csproj src/Shared/Shared.Contracts/
COPY src/Shared/Shared.Grpc/Shared.Grpc.csproj src/Shared/Shared.Grpc/
COPY src/Gateways/Gateway.API/Gateway.API.csproj src/Gateways/Gateway.API/

RUN dotnet restore src/Gateways/Gateway.API/Gateway.API.csproj

COPY src/ src/

RUN dotnet publish src/Gateways/Gateway.API/Gateway.API.csproj \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "Gateway.API.dll"]

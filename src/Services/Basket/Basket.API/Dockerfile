FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ServiceDefaults/ServiceDefaults.csproj src/ServiceDefaults/
COPY src/Shared/Shared.BuildingBlocks/Shared.BuildingBlocks.csproj src/Shared/Shared.BuildingBlocks/
COPY src/Shared/Shared.Contracts/Shared.Contracts.csproj src/Shared/Shared.Contracts/
COPY src/Shared/Shared.Grpc/Shared.Grpc.csproj src/Shared/Shared.Grpc/
COPY src/Services/Basket/Basket.API/Basket.API.csproj src/Services/Basket/Basket.API/
COPY src/Services/Basket/Basket.Application/Basket.Application.csproj src/Services/Basket/Basket.Application/
COPY src/Services/Basket/Basket.Domain/Basket.Domain.csproj src/Services/Basket/Basket.Domain/
COPY src/Services/Basket/Basket.Infrastructure/Basket.Infrastructure.csproj src/Services/Basket/Basket.Infrastructure/

RUN dotnet restore src/Services/Basket/Basket.API/Basket.API.csproj

COPY src/ src/

RUN dotnet publish src/Services/Basket/Basket.API/Basket.API.csproj \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "Basket.API.dll"]

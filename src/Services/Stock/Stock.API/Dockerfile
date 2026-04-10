FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ServiceDefaults/ServiceDefaults.csproj src/ServiceDefaults/
COPY src/Shared/Shared.BuildingBlocks/Shared.BuildingBlocks.csproj src/Shared/Shared.BuildingBlocks/
COPY src/Shared/Shared.Contracts/Shared.Contracts.csproj src/Shared/Shared.Contracts/
COPY src/Shared/Shared.Grpc/Shared.Grpc.csproj src/Shared/Shared.Grpc/
COPY src/Services/Stock/Stock.API/Stock.API.csproj src/Services/Stock/Stock.API/
COPY src/Services/Stock/Stock.Application/Stock.Application.csproj src/Services/Stock/Stock.Application/
COPY src/Services/Stock/Stock.Domain/Stock.Domain.csproj src/Services/Stock/Stock.Domain/
COPY src/Services/Stock/Stock.Infrastructure/Stock.Infrastructure.csproj src/Services/Stock/Stock.Infrastructure/

RUN dotnet restore src/Services/Stock/Stock.API/Stock.API.csproj

COPY src/ src/

RUN dotnet publish src/Services/Stock/Stock.API/Stock.API.csproj \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "Stock.API.dll"]

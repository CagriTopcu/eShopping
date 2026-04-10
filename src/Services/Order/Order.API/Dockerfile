FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ServiceDefaults/ServiceDefaults.csproj src/ServiceDefaults/
COPY src/Shared/Shared.BuildingBlocks/Shared.BuildingBlocks.csproj src/Shared/Shared.BuildingBlocks/
COPY src/Shared/Shared.Contracts/Shared.Contracts.csproj src/Shared/Shared.Contracts/
COPY src/Shared/Shared.Grpc/Shared.Grpc.csproj src/Shared/Shared.Grpc/
COPY src/Services/Order/Order.API/Order.API.csproj src/Services/Order/Order.API/
COPY src/Services/Order/Order.Application/Order.Application.csproj src/Services/Order/Order.Application/
COPY src/Services/Order/Order.Domain/Order.Domain.csproj src/Services/Order/Order.Domain/
COPY src/Services/Order/Order.Infrastructure/Order.Infrastructure.csproj src/Services/Order/Order.Infrastructure/

RUN dotnet restore src/Services/Order/Order.API/Order.API.csproj

COPY src/ src/

RUN dotnet publish src/Services/Order/Order.API/Order.API.csproj \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "Order.API.dll"]

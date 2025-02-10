FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InventoryManagement.API/InventoryManagement.API.csproj", "InventoryManagement.API/"]
COPY ["InventoryManagement.Core/InventoryManagement.Core.csproj", "InventoryManagement.Core/"]
COPY ["InventoryManagement.Infrastructure/InventoryManagement.Infrastructure.csproj", "InventoryManagement.Infrastructure/"]
RUN dotnet restore "InventoryManagement.API/InventoryManagement.API.csproj"
COPY . .
WORKDIR "/src/InventoryManagement.API"
RUN dotnet build "InventoryManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InventoryManagement.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryManagement.API.dll"]
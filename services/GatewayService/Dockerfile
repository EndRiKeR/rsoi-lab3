FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY . .
WORKDIR /app/services/GatewayService
RUN dotnet publish GatewayService.csproj --configuration Release --output ./publish
WORKDIR /app/services/GatewayService/publish
ENTRYPOINT ["dotnet", "GatewayService.dll"]
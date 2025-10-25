FROM  mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY services/BonusService/ ./BonusService/
COPY services/Common/ ./Common/
WORKDIR /app/BonusService
RUN dotnet publish -c Release -o ./publish
WORKDIR /app/BonusService/publish
ENTRYPOINT ["dotnet", "BonusService.dll"]
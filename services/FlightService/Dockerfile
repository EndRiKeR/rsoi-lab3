FROM  mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY services/FlightService/ ./FlightService/
COPY services/Common/ ./Common/
WORKDIR /app/FlightService
RUN dotnet publish -c Release -o ./publish
WORKDIR /app/FlightService/publish
ENTRYPOINT ["dotnet", "FlightService.dll"]
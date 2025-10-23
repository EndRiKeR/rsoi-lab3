FROM  mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY services/TicketsService/ ./TicketsService/
COPY services/Common/ ./Common/
WORKDIR /app/TicketsService
RUN dotnet publish -c Release -o ./publish
WORKDIR /app/TicketsService/publish
ENTRYPOINT ["dotnet", "TicketsService.dll"]
using Common.CircuitBreaker;
using FlightService.Controllers.Fallbacks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<CircuitBreakersController>();
builder.Services.AddSingleton<ControllersFallbacks>();

builder.Services.AddHttpClient("FlightService", client =>
{
    client.BaseAddress = new Uri("http://flight-service:8060");
});

builder.Services.AddHttpClient("TicketsService", client =>
{
    client.BaseAddress = new Uri("http://tickets-service:8070");
});

builder.Services.AddHttpClient("BonusService", client =>
{
    client.BaseAddress = new Uri("http://bonus-service:8050");
});

var app = builder.Build();

app.UseRouting();
app.MapControllers();

app.Run();

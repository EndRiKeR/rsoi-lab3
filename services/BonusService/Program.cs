using BonusService.Database;
using BonusService.Database.Repositories;
using BonusService.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("Gateway", client =>
{
    client.BaseAddress = new Uri("http://gateway-service:8080");
});

var connectionString = Environment.GetEnvironmentVariable("DOCKER_CONNECT_STRING") 
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PrivilegeContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddTransient<IPrivilegeRepository, PrivilegeRepository>();
builder.Services.AddTransient<IPrivilegeHistoryRepository, PrivilegeHistoryRepository>();

var app = builder.Build();

var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<PrivilegeContext>();
var pendingMigrations = context.Database.GetPendingMigrations().ToList();
if (pendingMigrations.Any())
{
    Console.WriteLine($"Applying {pendingMigrations.Count} migrations...");
    context.Database.Migrate();
    Console.WriteLine("Migrations applied successfully");
}
else
{
    Console.WriteLine("Database is up-to-date");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();

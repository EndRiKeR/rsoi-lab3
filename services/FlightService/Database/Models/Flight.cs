namespace FlightService.Database.Models;

public class Flight
{
    public int Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public int Price { get; set; }
    public int? FromAirportId { get; set; }
    public int? ToAirportId { get; set; }

    public virtual Airport? FromAirport { get; set; }
    public virtual Airport? ToAirport { get; set; }
}
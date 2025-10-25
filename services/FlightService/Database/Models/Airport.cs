namespace FlightService.Database.Models;

public class Airport
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public virtual ICollection<Flight> DepartureFlights { get; set; } = new List<Flight>();
    public virtual ICollection<Flight> ArrivalFlights { get; set; } = new List<Flight>();
}
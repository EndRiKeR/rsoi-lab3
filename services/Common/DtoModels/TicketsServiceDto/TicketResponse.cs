using System.Text.Json.Serialization;

namespace Common.DtoModels.TicketsServiceDto
{
    // Tickets Service DTO
    public class TicketResponse
    {
        [JsonPropertyName("ticketUid")]
        public Guid TicketUid { get; set; }
        
        [JsonPropertyName("flightNumber")]
        public string FlightNumber { get; set; } = string.Empty;
        
        [JsonPropertyName("fromAirport")]
        public string FromAirport { get; set; } = string.Empty;
        
        [JsonPropertyName("toAirport")]
        public string ToAirport { get; set; } = string.Empty;
        
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        
        [JsonPropertyName("price")]
        public int Price { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = "PAID";
    }
}
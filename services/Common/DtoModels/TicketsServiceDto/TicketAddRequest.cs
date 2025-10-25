using System.Text.Json.Serialization;

namespace Common.DtoModels.TicketsServiceDto
{
    public class TicketAddRequest
    {
        [JsonPropertyName("ticketUid")]
        public Guid TicketUid { get; set; }
        
        [JsonPropertyName("username")]
        public string Username { get; set; }
        
        [JsonPropertyName("flightNumber")]
        public string FlightNumber { get; set; }
        
        [JsonPropertyName("price")]
        public int Price { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
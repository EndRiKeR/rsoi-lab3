using System.Text.Json.Serialization;

namespace Common.DtoModels.TicketsServiceDto
{
    public class TicketPurchaseRequest
    {
        [JsonPropertyName("flightNumber")]
        public string FlightNumber { get; set; } = string.Empty;
        
        [JsonPropertyName("price")]
        public int Price { get; set; }
        
        [JsonPropertyName("paidFromBalance")]
        public bool PaidFromBalance { get; set; }
    }
}
using System.Text.Json.Serialization;
using Common.DtoModels.BonusServiceDto;

namespace Common.DtoModels.TicketsServiceDto
{
    public class TicketPurchaseResponse
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
        
        [JsonPropertyName("paidByMoney")]
        public int PaidByMoney { get; set; }
        
        [JsonPropertyName("paidByBonuses")]
        public int PaidByBonuses { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = "PAID";
        
        [JsonPropertyName("privilege")]
        public PrivilegeShortInfo Privilege { get; set; } = new();
    }
}
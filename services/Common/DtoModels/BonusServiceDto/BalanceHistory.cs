using System.Text.Json.Serialization;

namespace Common.DtoModels.BonusServiceDto
{
    public class BalanceHistory
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        
        [JsonPropertyName("ticketUid")]
        public Guid TicketUid { get; set; }
        
        [JsonPropertyName("balanceDiff")]
        public int BalanceDiff { get; set; }
        
        [JsonPropertyName("operationType")]
        public string OperationType { get; set; } = string.Empty;
    }
}
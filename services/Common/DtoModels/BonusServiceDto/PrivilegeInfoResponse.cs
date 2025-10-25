using System.Text.Json.Serialization;

namespace Common.DtoModels.BonusServiceDto
{

    public class PrivilegeInfoResponse
    {
        [JsonPropertyName("balance")]
        public int Balance { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = "BRONZE";
        
        [JsonPropertyName("history")]
        public List<BalanceHistory> History { get; set; } = new();
    }
}
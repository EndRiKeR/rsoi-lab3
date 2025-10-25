using System.Text.Json.Serialization;

namespace Common.DtoModels.BonusServiceDto
{
    // Bonus Service DTO
    public class PrivilegeShortInfo
    {
        [JsonPropertyName("balance")]
        public int Balance { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
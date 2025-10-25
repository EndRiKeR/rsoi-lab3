using System.Text.Json.Serialization;

namespace Common.DtoModels.ErrorDto
{
    public class ErrorDescription
    {
        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;
        
        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;
    }
}
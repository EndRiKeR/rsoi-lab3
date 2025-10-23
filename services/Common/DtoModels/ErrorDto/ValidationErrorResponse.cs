using System.Text.Json.Serialization;

namespace Common.DtoModels.ErrorDto
{
    public class ValidationErrorResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        
        [JsonPropertyName("errors")]
        public List<ErrorDescription> Errors { get; set; } = new();
    }
}
using System.Text.Json.Serialization;

namespace Common.DtoModels.ErrorDto
{
    // Error DTO
    public class ErrorResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}
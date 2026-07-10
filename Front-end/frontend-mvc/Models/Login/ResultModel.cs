using System.Text.Json.Serialization;

namespace Front_End.Models.Login
{
    public class ResultModel
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}

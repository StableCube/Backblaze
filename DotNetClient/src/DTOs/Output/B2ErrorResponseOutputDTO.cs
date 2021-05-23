using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class B2ErrorResponseOutputDTO
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        public override string ToString()
        {
            return $"Code: {Code}, Message: {Message}, Status: {Status}";
        }
    }
}
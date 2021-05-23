using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class ServerSideEncryptionOutputDTO
    {
        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("algorithm")]
        public string Algorithm { get; set; }

    }
}
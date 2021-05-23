using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class FileRetentionValueOutputDTO
    {
        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("retainUntilTimestamp")]
        public string RetainUntilTimestamp { get; set; }

    }
}
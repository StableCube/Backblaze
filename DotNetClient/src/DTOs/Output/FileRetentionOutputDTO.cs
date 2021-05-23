using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class FileRetentionOutputDTO
    {
        [JsonPropertyName("isClientAuthorizedToRead")]
        public bool IsClientAuthorizedToRead { get; set; }

        [JsonPropertyName("value")]
        public FileRetentionValueOutputDTO Value { get; set; }

    }
}
using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class FileLockLegalHoldOutputDTO
    {
        [JsonPropertyName("isClientAuthorizedToRead")]
        public bool IsClientAuthorizedToRead { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

    }
}
using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class UploadUrlOutputDTO
    {
        [JsonPropertyName("bucketId")]
        public string BucketId { get; set; }

        [JsonPropertyName("uploadUrl")]
        public string Url { get; set; }

        [JsonPropertyName("authorizationToken")]
        public string AuthorizationToken { get; set; }
    }
}
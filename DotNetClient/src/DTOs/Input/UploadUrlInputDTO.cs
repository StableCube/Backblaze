using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class UploadUrlInputDTO
    {
        [JsonPropertyName("bucketId")]
        public string BucketId { get; set; }
    }
}
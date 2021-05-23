using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class StartLargeFileUploadInputDTO
    {
        [JsonPropertyName("bucketId")]
        public string BucketId { get; set; }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }
    }
}
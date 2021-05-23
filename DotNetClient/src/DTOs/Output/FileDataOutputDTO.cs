using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class FileDataOutputDTO
    {
        [JsonPropertyName("accountId")]
        public string AccountId { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("bucketId")]
        public string BucketId { get; set; }

        [JsonPropertyName("contentLength")]
        public ulong ContentLength { get; set; }

        [JsonPropertyName("contentSha1")]
        public string ContentSha1 { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("fileId")]
        public string FileId { get; set; }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonPropertyName("uploadTimestamp")]
        public ulong UploadTimestamp { get; set; }
    }
}
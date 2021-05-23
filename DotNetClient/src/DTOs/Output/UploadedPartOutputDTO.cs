using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class UploadedPartOutputDTO
    {
        [JsonPropertyName("fileId")]
        public string FileId { get; set; }

        [JsonPropertyName("partNumber")]
        public int PartNumber { get; set; }

        [JsonPropertyName("contentLength")]
        public long ContentLength { get; set; }

        [JsonPropertyName("contentSha1")]
        public string ContentSha1 { get; set; }

        [JsonPropertyName("uploadTimestamp")]
        public long UploadTimestamp { get; set; }
    }
}
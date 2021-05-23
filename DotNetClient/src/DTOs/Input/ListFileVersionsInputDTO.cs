using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class ListFileVersionsInputDTO
    {
        [JsonPropertyName("bucketId")]
        public string BucketId { get; set; }

        [JsonPropertyName("startFileName")]
        public string StartFileName { get; set; }

        [JsonPropertyName("startFileId")]
        public string StartFileId { get; set; }

        [JsonPropertyName("maxFileCount")]
        public int? MaxFileCount { get; set; }

        [JsonPropertyName("prefix")]
        public string Prefix { get; set; }

        [JsonPropertyName("delimiter")]
        public string Delimiter { get; set; }
    }
}
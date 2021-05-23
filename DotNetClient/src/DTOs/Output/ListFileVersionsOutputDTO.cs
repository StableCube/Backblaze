using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class ListFileVersionsOutputDTO
    {
        [JsonPropertyName("files")]
        public FileVersionOutputDTO[] Files { get; set; }

        [JsonPropertyName("nextFileName")]
        public string NextFileName { get; set; }

        [JsonPropertyName("nextFileId")]
        public string NextFileId { get; set; }
    }
}
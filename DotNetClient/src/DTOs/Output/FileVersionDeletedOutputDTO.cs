using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class FileVersionDeletedOutputDTO
    {
        [JsonPropertyName("fileId")]
        public string FileId { get; set; }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; }
    }
}
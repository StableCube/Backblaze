using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class UploadPartUrlInputDTO
    {
        [JsonPropertyName("fileId")]
        public string FileId { get; set; }
    }
}
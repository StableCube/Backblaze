using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class FinishLargeFileUploadInputDTO
    {
        [JsonPropertyName("fileId")]
        public string FileId { get; set; }

        [JsonPropertyName("partSha1Array")]
        public string[] PartSha1Array { get; set; }
    }
}
using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class DeleteFileVersionInputDTO
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonPropertyName("fileId")]
        public string FileId { get; set; }

        [JsonPropertyName("bypassGovernance")]
        public bool? BypassGovernance { get; set; }
    }
}
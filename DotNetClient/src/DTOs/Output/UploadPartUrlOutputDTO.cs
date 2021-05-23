using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class UploadPartUrlOutputDTO
    {
        [JsonPropertyName("fileId")]
        public string FileId { get; set; }

        [JsonPropertyName("uploadUrl")]
        public string Url { get; set; }

        [JsonPropertyName("authorizationToken")]
        public string AuthorizationToken { get; set; }
    }
}
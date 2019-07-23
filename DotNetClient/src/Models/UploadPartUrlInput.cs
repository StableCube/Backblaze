using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class UploadPartUrlInput
    {
        [JsonProperty("fileId")]
        public string FileId { get; set; }
    }
}
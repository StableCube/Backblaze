using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class UploadUrlInput
    {
        [JsonProperty("bucketId")]
        public string BucketId { get; set; }
    }
}
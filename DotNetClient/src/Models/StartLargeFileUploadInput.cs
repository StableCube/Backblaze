using System;
using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class StartLargeFileUploadInput
    {
        [JsonProperty("bucketId")]
        public string BucketId { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }
    }
}
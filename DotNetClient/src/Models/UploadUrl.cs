using System;
using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class UploadUrl
    {
        [JsonProperty("bucketId")]
        public string BucketId { get; set; }

        [JsonProperty("uploadUrl")]
        public string Url { get; set; }

        [JsonProperty("authorizationToken")]
        public string AuthorizationToken { get; set; }
    }
}
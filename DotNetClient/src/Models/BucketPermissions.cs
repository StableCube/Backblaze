using System;
using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class BucketPermissions
    {
        [JsonProperty("bucketId")]
        public string BucketId { get; set; }

        [JsonProperty("bucketName")]
        public string BucketName { get; set; }

        [JsonProperty("capabilities")]
        public string[] Capabilities { get; set; }

        [JsonProperty("namePrefix")]
        public string NamePrefix { get; set; }
    }
}
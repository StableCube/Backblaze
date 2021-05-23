using System;
using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class BucketPermissionsOutputDTO
    {
        [JsonPropertyName("bucketId")]
        public string BucketId { get; set; }

        [JsonPropertyName("bucketName")]
        public string BucketName { get; set; }

        [JsonPropertyName("capabilities")]
        public string[] Capabilities { get; set; }

        [JsonPropertyName("namePrefix")]
        public string NamePrefix { get; set; }
    }
}
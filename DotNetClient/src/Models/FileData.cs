using System;
using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class FileData
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("bucketId")]
        public string BucketId { get; set; }

        [JsonProperty("contentLength")]
        public ulong ContentLength { get; set; }

        [JsonProperty("contentSha1")]
        public string ContentSha1 { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("fileId")]
        public string FileId { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("uploadTimestamp")]
        public ulong UploadTimestamp { get; set; }
    }
}
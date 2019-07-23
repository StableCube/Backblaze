using System;
using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class UploadedPart
    {
        [JsonProperty("fileId")]
        public string FileId { get; set; }

        [JsonProperty("partNumber")]
        public int PartNumber { get; set; }

        [JsonProperty("contentLength")]
        public string ContentLength { get; set; }

        [JsonProperty("contentSha1")]
        public string ContentSha1 { get; set; }

        [JsonProperty("uploadTimestamp")]
        public long UploadTimestamp { get; set; }
    }
}
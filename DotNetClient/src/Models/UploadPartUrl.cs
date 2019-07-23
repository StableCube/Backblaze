using System;
using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class UploadPartUrl
    {
        [JsonProperty("fileId")]
        public string FileId { get; set; }

        [JsonProperty("uploadUrl")]
        public string Url { get; set; }

        [JsonProperty("authorizationToken")]
        public string AuthorizationToken { get; set; }
    }
}
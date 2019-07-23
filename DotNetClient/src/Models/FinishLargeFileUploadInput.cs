using System;
using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class FinishLargeFileUploadInput
    {
        [JsonProperty("fileId")]
        public string FileId { get; set; }

        [JsonProperty("partSha1Array")]
        public string[] PartSha1Array { get; set; }
    }
}
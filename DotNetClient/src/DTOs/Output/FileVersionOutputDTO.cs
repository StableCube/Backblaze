using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class FileVersionOutputDTO
    {
        [JsonPropertyName("accountId")]
        public string AccountId { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("bucketId")]
        public string BucketId { get; set; }

        [JsonPropertyName("contentLength")]
        public ulong ContentLength { get; set; }

        [JsonPropertyName("contentSha1")]
        public string ContentSha1 { get; set; }

        [JsonPropertyName("contentMd5")]
        public string ContentMd5 { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("fileId")]
        public string FileId { get; set; }

        [JsonPropertyName("fileInfo")]
        public Dictionary<string, string> FileInfo { get; set; }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonPropertyName("fileRetention")]
        public FileRetentionOutputDTO FileRetention { get; set; }

        [JsonPropertyName("legalHold")]
        public FileLockLegalHoldOutputDTO LegalHold { get; set; }

        [JsonPropertyName("serverSideEncryption")]
        public ServerSideEncryptionOutputDTO ServerSideEncryption { get; set; }

        [JsonPropertyName("uploadTimestamp")]
        public long UploadTimestamp { get; set; }
    }
}
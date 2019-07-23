using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class Authorization
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("authorizationToken")]
        public string AuthorizationToken { get; set; }

        [JsonProperty("allowed")]
        public BucketPermissions Allowed { get; set; }

        [JsonProperty("apiUrl")]
        public string ApiUrl { get; set; }

        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonProperty("recommendedPartSize")]
        public long RecommendedPartSize { get; set; }

        [JsonProperty("absoluteMinimumPartSize")]
        public long AbsoluteMinimumPartSize { get; set; }

        [JsonProperty("minimumPartSize")]
        public long MinimumPartSize { get; set; }

        public bool HasWriteFilePermission()
        {
            return HasPermission("writeFiles");
        }

        public bool HasDeleteFilePermission()
        {
            return HasPermission("deleteFiles");
        }

        public bool HasPermission(string permission)
        {
            if(Allowed == null || Allowed.Capabilities == null)
                return false;

            foreach (var capability in Allowed.Capabilities)
            {
                if(capability == permission)
                    return true;
            }

            return false;
        }
    }
}
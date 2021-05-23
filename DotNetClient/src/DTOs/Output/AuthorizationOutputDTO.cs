using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class AuthorizationOutputDTO
    {
        [JsonPropertyName("accountId")]
        public string AccountId { get; set; }

        [JsonPropertyName("authorizationToken")]
        public string AuthorizationToken { get; set; }

        [JsonPropertyName("allowed")]
        public BucketPermissionsOutputDTO Allowed { get; set; }

        [JsonPropertyName("apiUrl")]
        public string ApiUrl { get; set; }

        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonPropertyName("recommendedPartSize")]
        public long RecommendedPartSize { get; set; }

        [JsonPropertyName("absoluteMinimumPartSize")]
        public long AbsoluteMinimumPartSize { get; set; }

        [JsonPropertyName("minimumPartSize")]
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
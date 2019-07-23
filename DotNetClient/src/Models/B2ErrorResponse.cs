using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class B2ErrorResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        public override string ToString()
        {
            return $"Code: {Code}, Message: {Message}, Status: {Status}";
        }
    }
}
using System.Net.Http;

namespace StableCube.Backblaze.DotNetClient
{
    public class BackblazeApiResponse<T>
    {
        public T Data { get; set; }

        public bool Succeeded { get; set; }

        public B2ErrorResponseOutputDTO Error { get; set; }
    }
}
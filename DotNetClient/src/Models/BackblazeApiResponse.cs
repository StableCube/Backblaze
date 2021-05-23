using System.Net.Http;

namespace StableCube.Backblaze.DotNetClient
{
    public class BackblazeApiResponse<T>
    {
        public HttpResponseMessage HttpResponse { get; set; }

        public T Data { get; set; }

        public bool Succeeded { get; set; }

        public B2ErrorResponseOutputDTO Error { get; set; }
    }
}
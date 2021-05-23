using System;

namespace StableCube.Backblaze.DotNetClient
{
    public class B2Exception : Exception
    {
        public B2Exception()
        {
        }

        public B2Exception(string message) : base(message)
        {
        }

        public B2Exception(B2ErrorResponseOutputDTO error) : base(error.ToString())
        {
        }

        public B2Exception(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

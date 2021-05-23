using System;

namespace StableCube.Backblaze.DotNetClient
{
    public class B2BadAuthTokenException : B2Exception
    {
        public B2BadAuthTokenException(B2ErrorResponseOutputDTO error) : base(error)
        {
        }
    }
}

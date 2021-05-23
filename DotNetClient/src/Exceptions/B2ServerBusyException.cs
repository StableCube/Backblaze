using System;

namespace StableCube.Backblaze.DotNetClient
{
    public class B2ServerBusyException : B2Exception
    {
        public B2ServerBusyException(B2ErrorResponseOutputDTO error) : base(error)
        {
        }
    }
}

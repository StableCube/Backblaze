using System;

namespace StableCube.Backblaze.DotNetClient
{
    public class B2ServerBusyException : B2Exception
    {
        public B2ServerBusyException(B2ErrorResponse error) : base(error)
        {
        }
    }
}

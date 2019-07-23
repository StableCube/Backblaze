using System;

namespace StableCube.Backblaze.DotNetClient
{
    public class B2HashMismatchException : B2Exception
    {
        public B2HashMismatchException(B2ErrorResponse error) : base(error)
        {
        }
    }
}

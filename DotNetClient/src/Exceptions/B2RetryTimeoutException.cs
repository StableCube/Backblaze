using System;

namespace StableCube.Backblaze.DotNetClient
{
    public class B2RetryTimeoutException : B2Exception
    {
        public B2RetryTimeoutException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

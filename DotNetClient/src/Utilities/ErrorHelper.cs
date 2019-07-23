using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public static class ErrorHelper
    {
        public static B2Exception GetExceptionForError(B2ErrorResponse error)
        {
            switch (error.Status)
            {
                case 400:
                    if(error.Message == "Sha1 did not match data received")
                    {
                        return new B2HashMismatchException(error);
                    }
                    else
                    {
                        return new B2Exception(error);
                    }
                case 401:
                    throw new B2BadAuthTokenException(error);
                case 503:
                    if(error.Code == "service_unavailable")
                    {
                        return new B2ServerBusyException(error);
                    }
                    else
                    {
                        return new B2Exception(error);
                    }
                default:
                    return new B2Exception(error);
            }
        }

        public static void ThrowException(string errorJson)
        {
            B2ErrorResponse error = JsonConvert.DeserializeObject<B2ErrorResponse>(errorJson);
            throw GetExceptionForError(error);
        }

        public static void ThrowException(B2ErrorResponse error)
        {
            throw GetExceptionForError(error);
        }

        /// <summary>
        /// Can an upload potentially be recovered by retrying after supplied error
        /// </summary>
        public static bool IsRecoverableError(B2ErrorResponse error)
        {
            var exception = GetExceptionForError(error);
            if(exception is B2HashMismatchException)
            {
                return true;
            }
            else if(exception is B2BadAuthTokenException)
            {
                return true;
            }
            else if(exception is B2ServerBusyException)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Exceptions that can potentially be recovered from by retrying
        /// </summary>
        public static bool IsRecoverableException(Exception exception)
        {
            if(exception is B2HashMismatchException)
            {
                return true;
            }
            else if(exception is B2BadAuthTokenException)
            {
                return true;
            }
            else if(exception is B2ServerBusyException)
            {
                return true;
            }
            else if(exception is HttpRequestException)
            {
                return true;
            }

            return false;
        }
    }
}
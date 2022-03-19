using System;

namespace WebStoreAPI.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message)
            : base(message) { }
    }
}
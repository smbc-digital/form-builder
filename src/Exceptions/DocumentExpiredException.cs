using System;

namespace form_builder.Exceptions
{
    public class DocumentExpiredException : Exception
    {
        public DocumentExpiredException(string message)
            : base(message)
        {
        }
    }
}
using System;

namespace JsonApiArchitecture.Exceptions
{
    public class JsonApiArchitectureException : Exception
    {
        public JsonApiArchitectureException(string message): base(message)
        {

        }
    }
}

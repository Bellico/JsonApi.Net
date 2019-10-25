using JsonApi.Components;
using JsonApi.Core;
using System;

namespace JsonApi.Exceptions
{
    public class InvalidPropertyException : JsonApiException
    {
        public InvalidPropertyException(string name, string actual, string expected)
        {
            this._error.Create(new JsonApiErrorObject()
            {
                status = Constants.ERROR_STATUT_INVALID_PROPERTY_EXCEPTION,
                title = $"Invalid Property {name} : {actual}",
                detail = $"{expected} is expected"
            });
        }

        public InvalidPropertyException(string name, string actual, string expected, string path)
        {
            this._error.Create(new JsonApiErrorObject()
            {
                status = Constants.ERROR_STATUT_INVALID_PROPERTY_EXCEPTION,
                title = $"Invalid Property {name} : {actual}",
                detail = $"{expected} is expected in : {path}"
            });
        }

        public InvalidPropertyException(Type type, string prop)
        {
            this._error.Create(new JsonApiErrorObject()
            {
                status = Constants.ERROR_STATUT_INVALID_PROPERTY_EXCEPTION,
                title = $"Invalid Property {prop}",
                detail = $"{type.Name} has not property {prop}"
            });
        }

        public InvalidPropertyException(Type type, string relationship, bool is_relationship)
        {
            this._error.Create(new JsonApiErrorObject()
            {
                status = Constants.ERROR_STATUT_MISSING_PROPERTY_EXCEPTION,
                title = $"Invalid relationships {relationship}",
                detail = $"{type.Name} has not relationships named {relationship}"
            });
        }
    }
}

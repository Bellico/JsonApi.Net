using JsonApi.Components;
using JsonApi.Core;

namespace JsonApi.Exceptions
{
    public class MissingPropertyException : JsonApiException
    {
        public MissingPropertyException(string propAwaiting, string source)
        {
            this._error.Create(new JsonApiErrorObject()
            {
                status = Constants.ERROR_STATUT_MISSING_PROPERTY_EXCEPTION,
                title = "Missing Property",
                detail = $"Property {propAwaiting} is Missing",
                source = source
            });
        }
    }
}
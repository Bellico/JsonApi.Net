using JsonApi.Components;
using JsonApi.Core;

namespace JsonApi.Exceptions
{
    class EmptyJsonObjectException : JsonApiException
    {
        public EmptyJsonObjectException()
        {
            this._error.Create(new JsonApiErrorObject()
            {
                status = Constants.ERROR_STATUT_EMPTY_EXCEPTION,
                title = "Json Object is empty",
            });
        }
    }
}
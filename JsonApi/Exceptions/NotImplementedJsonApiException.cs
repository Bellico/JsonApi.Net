using JsonApi.Components;
using JsonApi.Core;

namespace JsonApi.Exceptions
{
    public class NotImplementedJsonApiException : JsonApiException
    {
        public NotImplementedJsonApiException()
        {
            this._error.Create(new JsonApiErrorObject()
            {
                status = Constants.ERROR_STATUT_NOT_IMPLEMENTED_EXCEPTION,
                title = "Not Implemented Exception"
            });
        }

        public NotImplementedJsonApiException(string message)
        {
            this._error.Create(new JsonApiErrorObject()
            {
                status = Constants.ERROR_STATUT_NOT_IMPLEMENTED_EXCEPTION,
                title = "Not Implemented Exception",
                detail = message
            });
        }
    }
}
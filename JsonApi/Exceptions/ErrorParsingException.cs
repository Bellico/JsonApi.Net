using JsonApi.Components;
using JsonApi.Core;
using Newtonsoft.Json;

namespace JsonApi.Exceptions
{
    public class ErrorParsingException : JsonApiException
    {
        public ErrorParsingException(JsonReaderException ex)
        {
            this._error.Create(new JsonApiErrorObject()
            {
                status = Constants.ERROR_STATUT_PARSING_EXCEPTION,
                title = "Error Syntax",
                source = ex.Path,
                detail = ex.Message,
            });
        }
    }
}

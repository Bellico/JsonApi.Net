using JsonApi.Core;
using System;

namespace JsonApi.Exceptions
{
    public class JsonApiException : Exception
    {
        protected JsonApiError _error;

        public JsonApiException()
        {
            this._error = new JsonApiError();
        }

        public JsonApiException(JsonApiError error)
        {
            this._error = error;
        }

        public JsonApiError Error => this._error;
    }
}

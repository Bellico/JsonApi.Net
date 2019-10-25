using JsonApi.Components;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace JsonApi.Core
{
    public class JsonApiError
    {
        private readonly List<JsonApiErrorObject> _errorsJsonApi = new List<JsonApiErrorObject>();

        public bool HasErrors()
        {
            return this._errorsJsonApi.Count > 0;
        }

        public void ClearErrors()
        {
            this._errorsJsonApi.Clear();
        }

        public List<JsonApiErrorObject> GetErrors()
        {
            return this._errorsJsonApi;
        }

        public JObject GetJsonErrors()
        {
            JArray listItem = new JArray();
            var errors = new JObject();
            Newtonsoft.Json.JsonSerializer serializer = Constants.GetJsonSerializer();

            foreach (JsonApiErrorObject item in this._errorsJsonApi)
            {
                listItem.Add(JObject.FromObject(item, serializer));
            }

            errors.Add("errors", listItem);

            return errors;
        }

        public void Create(JsonApiErrorObject err)
        {
            this._errorsJsonApi.Add(err);
        }

        public void Create(string title, string detail, string source)
        {
            this._errorsJsonApi.Add(new JsonApiErrorObject() { status = Constants.ERROR_STATUT_CLIENT, title = title, detail = detail, source = source });
        }

        public void Create(int status, string title, string detail, string source)
        {
            this._errorsJsonApi.Add(new JsonApiErrorObject() { status = status, title = title, detail = detail, source = source });
        }
    }
}

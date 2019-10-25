using Newtonsoft.Json;

namespace JsonApi.Core
{
    public static class Constants
    {
        public const string JSON_PARAMETER_ACTION = "ressource";
        public const string JSON_ID_PARAMETER_ACTION = "id";
        public const string HTTP_QUERY_INCLUDE = "include";     
        public const int ERROR_STATUT_CLIENT = 400;
        public const int ERROR_STATUT_JSONAPI = 440;
        public const int ERROR_STATUT_EMPTY_EXCEPTION = 4401;
        public const int ERROR_STATUT_INVALID_PROPERTY_EXCEPTION = 4402;
        public const int ERROR_STATUT_MISSING_PROPERTY_EXCEPTION = 4403;
        public const int ERROR_STATUT_PARSING_EXCEPTION = 4044;
        public const int ERROR_STATUT_NOT_IMPLEMENTED_EXCEPTION = 501;
        public const int WAY_ALL = 0;
        public const int WAY_READER = 1;
        public const int WAY_PARSER = 2;
        public const string DEFAULT_ATTRIBUTE_ID = "Id";
        public const char SEPERATOR_IDS = '_';

        public static JsonSerializer GetJsonSerializer()
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonSerializer.Create(jsonSerializerSettings);
        }
    }
}

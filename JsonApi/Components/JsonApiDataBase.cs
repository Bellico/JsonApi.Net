using JsonApi.Interface;

namespace JsonApi.Components
{
    public class JsonApiDataBase : IJsonApiObject
    {
        public string id { get; set; }
        public string type { get; set; }
        public object toJsonFormat()
        {
            return this;
        }
    }
}

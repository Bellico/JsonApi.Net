using System.Collections.Generic;

namespace JsonApi.Components
{
    public class JsonApiData : JsonApiDataBase
    {
        public Dictionary<string, object> attributes { get; set; }
        public Dictionary<string, object> relationships { get; set; }

        public void AddAttribute(string name, object value)
        {
            if (this.attributes == null)
            {
                this.attributes = new Dictionary<string, object>();
            }

            if(value is System.DateTime)
            {
                value = ((System.DateTime)value).ToString("s");
            }

            this.attributes.Add(name, value);
        }
    
        public void AddRelationship(string name, object value)
        {
            if (this.relationships == null)
            {
                this.relationships = new Dictionary<string, object>();
            }

            this.relationships.Add(name, new
            {
                data = value
            });
        }
    }
}

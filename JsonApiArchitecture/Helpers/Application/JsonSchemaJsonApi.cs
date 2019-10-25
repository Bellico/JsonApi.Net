using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;

namespace JsonApiArchitecture.Helpers.Application
{
    public static class JsonSchemaJsonApi
    {
        public static JSchema Generate(Type type)
        {
            JSchemaGenerator generator = new JSchemaGenerator();

            return generator.Generate(type);
        }

        public static JObject GenerateWithJsonApi<T>()
        {
            return GenerateWithJsonApi(typeof(T));
        }

        public static JObject GenerateWithJsonApi(Type type)
        {
            JSchema schema = Generate(type);
            JObject source = JObject.FromObject(schema);

            var target = JObject.FromObject(new { definitions = new { }, properties = new { }, type = "object" });

            Transform(source, target);

            return target;
        }

        /// <summary>
        /// Parcours le JsonSchema et copie les mêmes élements dans la cible en changeant les clés des propriétés par les propriétés "title"
        /// </summary>
        private static void Transform(JObject source, JObject target)
        {
            // Parcours definitions/
            foreach (JProperty definition in source["definitions"]) {

                if (definition.First["title"] != null) {
                    string definitionTitle = definition.First["title"].ToString();

                    target["definitions"][definitionTitle] = new JObject {
                        { "properties", new JObject() },
                        { "type", definition.First["type"] }
                    };

                    // Parcours definitions/x/properties
                    foreach (JProperty property in definition.First["properties"]) {

                        if (property.First["title"] != null) {
                            target["definitions"][definitionTitle]["properties"][property.First["title"].ToString()] = property.First;
                        }

                        if (property.First["$ref"] != null) {

                            string path = property.First["$ref"].ToString().Replace("#", "$").Replace("/", ".");
                            string title = source.SelectToken(path)?["title"].ToString();

                            if (title != null) {
                                target["definitions"][definitionTitle]["properties"][title] = property.First;
                                target["definitions"][definitionTitle]["properties"][title]["$ref"].Replace($"#/definitions/{title}");
                            }
                        }

                        if (property.First["items"]?["$ref"] != null) {
                            string path = property.First["items"]["$ref"].ToString().Replace("#", "$").Replace("/", ".");
                            string title = source.SelectToken(path)?["title"].ToString();

                            if (title != null) {
                                target["definitions"][definitionTitle]["properties"][property.First["title"].ToString()]["items"]["$ref"].Replace($"#/definitions/{title}");
                            }
                        }
                    }

                }

            }

            // Parcours properties/
            foreach (JProperty property in source["properties"]) {

                if (property.First["title"] != null) {
                    target["properties"][property.First["title"].ToString()] = property.First;
                }

                if (property.First["items"]?["$ref"] != null) {
                    string path = property.First["items"]?["$ref"].ToString().Replace("#", "$").Replace("/", ".");
                    string title = source.SelectToken(path)?["title"].ToString();

                    if (title != null) {
                        target["properties"][property.First["title"].ToString()]["items"]?["$ref"].Replace($"#/definitions/{title}");
                    }
                }
            }
        }
    }
}

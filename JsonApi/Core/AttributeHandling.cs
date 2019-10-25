using JsonApi.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApi.Core
{
    public static class AttributeHandling
    {
        public static PropertyInfo GetIdProperty(Type type) 
            => type.GetProperties().FirstOrDefault(prop => prop.GetCustomAttribute<IdJsonApiAttribute>() != null) ?? type.GetProperty(Constants.DEFAULT_ATTRIBUTE_ID);
      
        public static IEnumerable<PropertyInfo> GetIdsProperties(Type type) 
            => type.GetProperties().Where(prop => prop.GetCustomAttribute<IdJsonApiAttribute>() != null || prop.Name == Constants.DEFAULT_ATTRIBUTE_ID);

        public static bool IsIgnoreJsonApi(PropertyInfo prop) => prop.GetCustomAttribute<IgnoreJsonApiAttribute>() != null;

        public static string GetLabelProperty(PropertyInfo prop) => prop.GetCustomAttribute<PropertyJsonApiAttribute>()?.Label ?? prop.Name;

        public static string GetLabelProperty(Type type) => type.GetCustomAttribute<PropertyJsonApiAttribute>()?.Label ?? type.Name.ToLower();

        public static Type GetTypeProperty(PropertyInfo prop) => prop.GetCustomAttribute<PropertyJsonApiAttribute>()?.Type ?? prop.PropertyType;

        public static string GetResolverName(PropertyInfo prop) => prop.GetCustomAttribute<ResolverJsonApiAttribute>()?.Name;

        public static IEnumerable<PropertyInfo> GetListeProperties(Type type, List<string> listeName)
        {
            foreach (string name in listeName) {
                PropertyInfo property = GetProperty(type, name);

                if (property != null)
                    yield return property;
            }
        }

        public static PropertyInfo GetProperty(Type type, string name)
        {
            PropertyInfo property = type.GetProperty(name);

            if (property != null) {
                IgnoreJsonApiAttribute ignoreAttr = property.GetCustomAttribute<IgnoreJsonApiAttribute>();
                return (ignoreAttr == null) ? property : null;
            }

            foreach (PropertyInfo prop in type.GetProperties()) {
                PropertyJsonApiAttribute attr = prop.GetCustomAttribute<PropertyJsonApiAttribute>();

                if (attr?.Label != null && attr?.Label == name) {
                    IgnoreJsonApiAttribute ignoreAttr = prop.GetCustomAttribute<IgnoreJsonApiAttribute>();
                    return (ignoreAttr == null) ? prop : null;
                }
            }

            return null;
        }

        public static bool IsIgnoreJsonApi(MethodInfo method, int way)
        {
            var attr = (IgnoreJsonApiAttribute)method.GetCustomAttribute(typeof(IgnoreJsonApiAttribute));

            if (attr == null)
                return false;

            if (way == Constants.WAY_ALL)
                return true;

            if (attr.IgnoreParser && way == Constants.WAY_PARSER)
                return true;

            if (attr.IgnoreReader && way == Constants.WAY_READER)
                return true;

            return false;
        }
    }
}

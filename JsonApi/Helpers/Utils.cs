using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JsonApi.Helpers
{
    public static class Utils
    {
        public static bool IsEnum(object obj) =>
            obj is IEnumerable && !(obj is string) && !(obj is IDictionary) && !(obj is List<KeyValuePair<string,string>>);
        

        public static bool IsTypeSystem(Type type)
        {
            try
            {
                return type.Namespace.StartsWith("System");
            } catch
            {
                return false;
            }
        }

        public static bool HasGenericTypeSystem(Type type)
        {
            Type typeGeneric = type.GetElementType();
            if(typeGeneric != null)
            {
                return IsTypeSystem(typeGeneric);
            }
            else
            {
                Type[] arguments = type.GetGenericArguments();

                if (arguments.Length == 0) return IsTypeSystem(type);

                return IsTypeSystem(arguments[0]);
            }
        }

        public static object FormatValueByType(Type type, string value)
        {
            if(string.IsNullOrEmpty(value)) return null;

            if (type == typeof(string))
            {
                return value;
            }

            if (type == typeof(int) || type == typeof(int?))
            {
                return int.Parse(value);
            }

            if (type == typeof(decimal) || type == typeof(decimal?))
            {
                return decimal.Parse(value);
            }

            if (type == typeof(bool) || type == typeof(bool?))
            {
                return (value.ToLower() == "true");
            }

            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return DateTime.Parse(value);
            }

            return null;
        }

        public static object FormatNullValue(Type type)
        {
            if (type == typeof(string)) {
                return "NULL";
            }

            if (type == typeof(int) || type == typeof(int?) || type == typeof(decimal) || type == typeof(decimal?)) {
                return -1;
            }

            if (type == typeof(bool) || type == typeof(bool?)) {
                return false;
            }

            if (type == typeof(DateTime) || type == typeof(DateTime?)) {
                return new DateTime();
            }

            return null;
        }

        public static object FormatListValueByType(Type type, List<string> values)
        {
            if (type == typeof(List<string>))
            {
                return values;
            }

            if (type == typeof(List<int>))
            {
                return values.Select(i => int.Parse(i)).ToList();
            }

            if (type == typeof(List<decimal>))
            {
                return values.Select(i => decimal.Parse(i)).ToList();
            }

            return null;
        }

        public static List<string> FormatQueryToInclude(ICollection<string> parameters)
        {
            var included = new List<string>();
            foreach (string item in parameters)
            {
                foreach (string keyword in item.Split(','))
                {
                    included.Add(keyword);
                }
            }

            return included ;
        }

        public static Dictionary<string, List<string>> FormatQueryToFields(IEnumerable<KeyValuePair<string, StringValues>> fieldsQuery)
        {
            var fields = new Dictionary<string, List<string>>();
            var regex = new System.Text.RegularExpressions.Regex(@"([A-Za-z_]+)");

            foreach (KeyValuePair<string, StringValues> item in fieldsQuery) {
                try {
                    string fieldname = regex.Matches(item.Key)?[1]?.Value;
                    fields.Add(fieldname, item.Value.ToString().Split(',').ToList());
                } catch {
                    return fields;
                }
            }

            return fields;
        }

        public static string FormatLinkPagination(int number, int size, string filter)
        {
            string query = "?page={number:" + number + ",size:" + size + "}";
            return (string.IsNullOrEmpty(filter)) ? query : query + "&" + filter;
        }

        public static bool IsRelationInInclude(string relationPath, List<string> includes)
        {
            if (includes != null) {

                if (includes.Contains("*"))
                    return true;

                if (includes.Any(i => i == relationPath || i.StartsWith(relationPath + ".")))
                    return true;
            }

            return false;
        }
    }
}

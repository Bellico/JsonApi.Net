using System;
using System.Reflection;

namespace JsonApiArchitecture.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static Type GetPropertyTypeSample(this PropertyInfo type)
        {
            return type.PropertyType.IsGenericType ? type.PropertyType.GetGenericArguments()[0] : type.PropertyType;
        }

        public static void SetValueAsId(this PropertyInfo propertyInfo, object obj, int value)
        {
            if (propertyInfo.PropertyType == typeof(decimal) || propertyInfo.PropertyType == typeof(decimal?))
                propertyInfo.SetValue(obj, (decimal)value);

            else
                propertyInfo.SetValue(obj, value);
        }
    }
}

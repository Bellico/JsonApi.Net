using JsonApi.Attributes;
using JsonApi.Core;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Linq;

namespace JsonApi.Extensions
{
    public static class ActionDescriptorExtensions
    {
        public static string GetParameterNameJsonApi(this ActionDescriptor actionDescriptor)
        {
            ParameterDescriptor jsonParameter = actionDescriptor.Parameters
                .FirstOrDefault(p =>( p as ControllerParameterDescriptor).ParameterInfo.CustomAttributes.Count(attr => attr.AttributeType == typeof(JsonApiAttribute)) == 1);

           return (jsonParameter != null) ? jsonParameter.Name : Constants.JSON_PARAMETER_ACTION;
        }

        public static string GetParameterNameIdArgument(this ActionDescriptor actionDescriptor)
        {
            ParameterDescriptor idParamater = actionDescriptor.Parameters
                .FirstOrDefault(p => (p as ControllerParameterDescriptor).ParameterInfo.CustomAttributes.Count(attr => attr.AttributeType == typeof(IdArgumentJsonApiAttribute)) == 1);

            return (idParamater != null) ? idParamater.Name : Constants.JSON_ID_PARAMETER_ACTION;
        }
    }
}

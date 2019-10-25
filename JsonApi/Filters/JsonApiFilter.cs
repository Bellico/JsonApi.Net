using JsonApi.Core;
using JsonApi.Exceptions;
using JsonApi.Extensions;
using JsonApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;

namespace JsonApi.Filter
{
    public class JsonApiFilter : ActionFilterAttribute
    {
        /// <summary>
        /// Execute on each request
        /// Convert Json Object in request body to the expected type in the controller action
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (AttributeHandling.IsIgnoreJsonApi(((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo, Constants.WAY_READER)) return;

            string parameter_name = context.ActionDescriptor.GetParameterNameJsonApi();
            object obj = context.ActionArguments.FirstOrDefault(i => i.Key == parameter_name).Value;

            if (obj != null)
            {
                try
                {
                    JsonApiService service = context.HttpContext.RequestServices.GetRequiredService<JsonApiService>();

                    JsonApiReader jsonReader = new JsonApiReader(new StreamReader(context.HttpContext.Request.Body).ReadToEnd(), obj);
                    jsonReader.SetResolver(new ResolverData(context.HttpContext.RequestServices, service.Resolvers));
                    jsonReader.SetConstraintId(context.ActionArguments.FirstOrDefault(i => i.Key == context.ActionDescriptor.GetParameterNameIdArgument()).Value, context.HttpContext.Request.Method);

                    context.ActionArguments[parameter_name] = jsonReader.GetModel();
                }
                catch (JsonApiException ex)
                {
                    context.HttpContext.Response.StatusCode = Constants.ERROR_STATUT_JSONAPI;
                    context.Result = new ObjectResult(ex.Error.GetJsonErrors());
                }
            }

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Execute after each action
        /// Convert return type to jsonAPI format
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (AttributeHandling.IsIgnoreJsonApi(((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo, Constants.WAY_PARSER)) return;

            JsonApiService service = context.HttpContext.RequestServices.GetRequiredService<JsonApiService>();

            if (service.Error != null && service.Error.HasErrors())
            {
                context.HttpContext.Response.StatusCode = Constants.ERROR_STATUT_JSONAPI;
                context.Result = new ObjectResult(service.Error.GetJsonErrors());

                return;
            }

            if (context.Result is ObjectResult)
            {
                object result = ((ObjectResult)context.Result).Value;

                //If result is already JObject, return it without transform
                if (result?.GetType() == typeof(Newtonsoft.Json.Linq.JObject)) return;

                //Transform result to JsonAPI format
                JsonApiParser jsonAPIParser = service.Parser;
                jsonAPIParser.SetResolver(new ResolverData(context.HttpContext.RequestServices, service.Resolvers));
                jsonAPIParser.SetModel(result);
                jsonAPIParser.SetOptionsWithQuery(context.HttpContext.Request.Query);

                ((ObjectResult)context.Result).Value = jsonAPIParser.GetJson();
            }

            base.OnActionExecuted(context);
        }
    }
}

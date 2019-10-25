using JsonApi.Interface;
using JsonApi.Services;
using JsonApiArchitecture.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiArchitecture.Filters
{
    public class JsonApiArchitectureFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var service = (JsonApiService)context.HttpContext.RequestServices.GetService(typeof(JsonApiService));
            var queryService = (QueryService)context.HttpContext.RequestServices.GetService(typeof(IQueryService));

            if (service.Error != null && service.Error.HasErrors()) {
                return;
            }

            if (context.Result is ObjectResult) {
                object result = ((ObjectResult)context.Result).Value;

                new RelationsEngine(context.HttpContext.RequestServices, queryService, result).BuildRelation();
            }

            base.OnActionExecuted(context);
        }
    }
}

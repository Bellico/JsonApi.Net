using JsonApi.Core;
using JsonApiArchitecture.Attributes;
using JsonApiArchitecture.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Linq;
using System.Reflection;
using V_UP.Api.Extensions;

namespace JsonApiArchitecture.Helpers.Application
{
    public class GenericControllerRouteConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.IsGenericType) {
                // Le parametre généric de la ressource est en premiere position
                Type primaryRessourceType = controller.ControllerType.GetPrimaryRessourceType();

                // Route à appliquer au controller
                string route;

                // Cas d'un Controlleur simple
                if (!controller.ControllerType.IsControllerRelation()) {
                    ControllerJsonApiAttribute controllerAttribute = primaryRessourceType.GetCustomAttribute<ControllerJsonApiAttribute>();
                    route = controllerAttribute.Route;

                 // Cas d'un Controlleur de relation
                } else {
                    Type secondaryRessourceType = controller.ControllerType.GetSecondaryRessourceType();

                    ControllerJsonApiAttribute controllerAttribute = secondaryRessourceType.GetCustomAttribute<ControllerJsonApiAttribute>();

                    PropertyInfo propertyRelation = secondaryRessourceType.GetProperties().FirstOrDefault(p => p.GetPropertyTypeSample() == primaryRessourceType);

                    if (controllerAttribute == null || propertyRelation == null)
                        throw new NotImplementedException();

                    route = controllerAttribute.Route + "/{id}/" + AttributeHandling.GetLabelProperty(propertyRelation);
                }

                controller.Selectors.Clear();
                controller.Selectors.Add(new SelectorModel {
                    AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(route))
                });
            }
        }
    }
}

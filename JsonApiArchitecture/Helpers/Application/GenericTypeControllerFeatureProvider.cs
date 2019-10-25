using JsonApiArchitecture.Api.Helpers.Application;
using JsonApiArchitecture.Attributes;
using JsonApiArchitecture.Core;
using JsonApiArchitecture.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using V_UP.Api.Extensions;

namespace JsonApiArchitecture.Helpers.Application
{
    public class GenericTypeControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public GenericTypeControllerFeatureProvider(Assembly assembly)
        {
            this.Assembly = assembly;
        }
        public Assembly Assembly { get; } 

        private List<TypeInfo> _controllersGeneric;

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            this._controllersGeneric = feature.Controllers.Where(c => c.BaseType.IsGenericType).ToList();

            this.PopulateRelationExistingController(feature);

            this.PopulateControllerRessource(feature);
        }

        /// <summary>
        /// Genere les controllers des relations des controllers existants
        /// </summary>
        public void PopulateRelationExistingController(ControllerFeature feature)
        {
            foreach (TypeInfo controller in this._controllersGeneric)
                this.PopulateControllerRelationShip(feature, controller.BaseType.GetPrimaryRessourceType());
        }

        /// <summary>
        /// Genere les nouveaux controllers des ressources
        /// </summary>
        public void PopulateControllerRessource(ControllerFeature feature)
        {
            IEnumerable<Type> ressourcesToController = this.Assembly.GetExportedTypes().Where(x => x.GetCustomAttribute<ControllerJsonApiAttribute>() != null);

            foreach (Type typeRessource in ressourcesToController) {

                if (this._controllersGeneric.Any(c => c.BaseType.GetPrimaryRessourceType() == typeRessource))
                    continue;

                ControllerJsonApiAttribute controllerAttribute = typeRessource.GetCustomAttribute<ControllerJsonApiAttribute>();
                feature.Controllers.Add(new ControllerMaker(typeRessource, controllerAttribute.ReadOnly).MakeController());

                this.PopulateControllerRelationShip(feature, typeRessource);
            }
        }

        /// <summary>
        /// Genere les controllers des relations ressources
        /// </summary>
        private void PopulateControllerRelationShip(ControllerFeature feature, Type typeRessource)
        {
            foreach (Tuple<PropertyInfo, RelationshipJsonApiAttribute> relation in AttributeRelationsHandling.GetRelationshipProperties(typeRessource)) {

                if (relation.Item2 is HasOneJsonApiAttribute)
                    continue;

                if (relation.Item1.GetPropertyTypeSample() == typeRessource)
                    continue;

                ControllerRelationJsonApiAttribute controllerRelationAttribute =
                    relation.Item1.GetCustomAttribute<ControllerRelationJsonApiAttribute>();

                if (controllerRelationAttribute != null && controllerRelationAttribute.None)
                    continue;

                feature.Controllers.Add(new ControllerMaker(typeRessource, relation.Item1.GetPropertyTypeSample(), controllerRelationAttribute?.ReadOnly ?? false).MakeRelationController());
            }
        }
    }
}

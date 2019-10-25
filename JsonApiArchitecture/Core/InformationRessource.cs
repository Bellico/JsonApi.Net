using JsonApi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonApiArchitecture.Core
{
    internal class InformationRessource
    {
        public object Ressource { get; }
        public bool IsEnum { get; }
        public Type TypeRessource { get; }

        public InformationRessource(object ressource)
        {
            if (ressource != null) {
                this.Ressource = ressource;
                this.IsEnum = Utils.IsEnum(ressource);
                this.TypeRessource =
                    this.IsEnum && ((IEnumerable<object>)ressource).Any() ?
                    ((IEnumerable<object>)ressource).First().GetType()
                    : ressource.GetType();
            }
        }

        public bool HaveType => this.TypeRessource != null;

        public IEnumerable<object> RessourceEnum => this.IsEnum ? this.Ressource as IEnumerable<object> : new List<object>() { this.Ressource };
    }
}

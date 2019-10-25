using JsonApiArchitecture.Attributes;
using JsonApiArchitecture.Helpers.BaseController;
using System;
using System.Reflection;

namespace JsonApiArchitecture.Api.Helpers.Application
{
    public class ControllerMaker
    {
        private readonly RessourceJsonApiAttribute _ressourceAttribute;
        private readonly Type _interfaceRessource;
        private readonly Type _interfaceCritere;
        private readonly Type _typeRessource;
        private readonly Type _typeRelationRessource;
        private readonly bool _isReadOnly;

        public ControllerMaker(Type typeRessource, bool isReadOnly)
        {
            this._typeRessource = typeRessource;
            this._isReadOnly = isReadOnly;

            // Récuperation de l'attribut Ressourcee
            this._ressourceAttribute = typeRessource.GetCustomAttribute<RessourceJsonApiAttribute>();
            // Récuperation de l'interface directement sur le type
            this._interfaceRessource = typeRessource.GetInterfaces()[0];
            // Récuperation de l'interface de criteres
            this._interfaceCritere = this._ressourceAttribute.Criteres.GetInterface("I" + this._ressourceAttribute.Criteres.Name);

            if (this._ressourceAttribute.Repository == null)
                this._isReadOnly = true;
        }


        public ControllerMaker(Type typeRelationRessource, Type typeRessource, bool isReadOnly) : this(typeRessource, isReadOnly)
        {
            this._typeRelationRessource = typeRelationRessource;
            this._isReadOnly = isReadOnly;
        }

        public TypeInfo MakeController()
        {
            Type controllerType = this._isReadOnly ? typeof(JsonApiReadOnlyController<,,,,>) : typeof(JsonApiController<,,,,,>);

            return controllerType.MakeGenericType(this._isReadOnly ? this.GetReadOnlyTypeArguments() : this.GetTypeArguments()).GetTypeInfo();
        }

        public TypeInfo MakeRelationController()
        {
            Type controllerType = this._isReadOnly ? typeof(JsonApiRelationReadOnlyController<,,,,,>) : typeof(JsonApiRelationController<,,,,,,>);

            return controllerType.MakeGenericType(this._isReadOnly ? this.GetReadOnlyRelationTypeArguments() : this.GetRelationTypeArguments()).GetTypeInfo();
        }

        /// <summary>
        /// Construction du tableau des types génériques nécéssaire à l'instanciation du controller
        /// Dans l'ordre suivant : TRessource, TCriteres, TIDataSource, TIRepository, TIRessource, TICriteres
        /// </summary>
        private Type[] GetTypeArguments()
        {
            return new Type[] {
                this._typeRessource,
                this._ressourceAttribute.Criteres,
                this._ressourceAttribute.DataSource,
                this._ressourceAttribute.Repository,
                this._interfaceRessource,
                this._interfaceCritere
            };
        }

        /// <summary>
        /// Construction du tableau des types génériques nécéssaire à l'instanciation du controller ReadOnly
        /// Dans l'ordre suivant : TRessource, TCriteres, TIDataSource, TIRessource, TICriteres
        /// </summary>
        private Type[] GetReadOnlyTypeArguments()
        {
            return new Type[] {
                this._typeRessource,
                this._ressourceAttribute.Criteres,
                this._ressourceAttribute.DataSource,
                this._interfaceRessource,
                this._interfaceCritere
            };
        }

        /// <summary>
        /// Construction du tableau des types génériques nécéssaire à l'instanciation du controller Relation ReadOnly
        /// Dans l'ordre suivant : TRessource, TRelationRessource, TCriteres, TIDataSource, TIRessource, TICriteres
        /// </summary>
        private Type[] GetReadOnlyRelationTypeArguments()
        {
            return new Type[] {
                this._typeRessource,
                this._typeRelationRessource,
                this._ressourceAttribute.Criteres,
                this._ressourceAttribute.DataSource,
                this._interfaceRessource,
                this._interfaceCritere,
            };
        }

        /// <summary>
        /// Construction du tableau des types génériques nécéssaire à l'instanciation du controller Relation
        /// Dans l'ordre suivant : TRessource, TRelationRessource, TCriteres, TIDataSource, TIRepository, TIRessource, TICriteres
        /// </summary>
        private Type[] GetRelationTypeArguments()
        {
            return new Type[] {
                this._typeRessource,
                this._typeRelationRessource,
                this._ressourceAttribute.Criteres,
                this._ressourceAttribute.DataSource,
                this._ressourceAttribute.Repository,
                this._interfaceRessource,
                this._interfaceCritere,
            };
        }
    }
}

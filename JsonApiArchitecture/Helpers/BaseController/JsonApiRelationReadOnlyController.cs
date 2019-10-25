using JsonApiArchitectureBase;
using JsonApiArchitectureBase.Interfaces;
using JsonApi.Core;
using JsonApi.Interface;
using JsonApiArchitecture.Core;
using JsonApiArchitecture.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JsonApiArchitecture.Helpers.BaseController
{
    /// <typeparam name="TRessource">Ressource principale</typeparam>
    /// <typeparam name="TRessourceRelation">Ressource parente</typeparam>
    public class JsonApiRelationReadOnlyController<TRessource, TRessourceRelation, TCriteres, TIDataSource, TIRessource, TICriteres> : ControllerBase
           where TRessource : TIRessource, new()
           where TCriteres : TICriteres, new()
           where TIDataSource : IDataSource<TIRessource, TICriteres>
    {
        public TIDataSource _datasource { get; }
        public IQueryService _queryService { get; }

        protected PropertyInfo PrimaryForeigneKeyProperty { get; }

        public JsonApiRelationReadOnlyController(TIDataSource dataSource, IQueryService queryService)
        {
            this._datasource = dataSource;
            this._queryService = queryService;

            this.PrimaryForeigneKeyProperty = AttributeRelationsHandling.FindForeignKeyPropertyFromType(typeof(TRessource), typeof(TRessourceRelation));
        }

        #region Virtual Methods
        /// <summary>
        /// Récupere une instance de classe de criteres pour la récupération d'une liste de ressource
        /// </summary>
        protected virtual TCriteres GetCriteresListe(int id)
        {
            TCriteres criteres = this._queryService.Filter<TCriteres>();

            PropertyInfo critereProperty = typeof(TCriteres).GetProperty(this.PrimaryForeigneKeyProperty.Name);

            if (critereProperty == null)
                throw new ArgumentException($"Ajouter un critere {this.PrimaryForeigneKeyProperty.Name} dans {typeof(TCriteres).Name}");

            critereProperty.SetValueAsId(criteres, id);

            return criteres;
        }

        /// <summary>
        /// Récupere une instance de classe de criteres pour la récupération d'une ressource
        /// </summary>
        protected virtual TCriteres GetCriteres(int id, int idRelation)
        {
            TCriteres criteres = this.GetCriteresListe(id);

            if (criteres is ICriteresBase)
                (criteres as ICriteresBase).Id = idRelation;
            else {
                PropertyInfo secondaryKey = this.FindSecondaryForeignKeyProperty();

                if (secondaryKey == null)
                    return default(TCriteres);

                PropertyInfo critereProperty = typeof(TCriteres).GetProperty(secondaryKey.Name);

                if (critereProperty == null)
                    throw new ArgumentException($"Ajouter un critere {secondaryKey.Name} dans {typeof(TCriteres).Name}");

                critereProperty.SetValueAsId(criteres, idRelation);
            }

            return criteres;
        }
        #endregion

        #region Helpers
        protected PropertyInfo FindSecondaryForeignKeyProperty()
        {
            IEnumerable<PropertyInfo> idsProperties = AttributeHandling.GetIdsProperties(typeof(TRessource));

            //Si plus de 2 clés primaire, on ne peut pas définir instinctivement la seconde clé étrangere
            if (idsProperties.Count() > 2)
                return null;

            return idsProperties.FirstOrDefault(prop => prop != this.PrimaryForeigneKeyProperty);
        }

        protected void AssignIdsInRessource(TRessource ressource, int id, int idRelation, out PropertyInfo secondaryForeigneKeyProperty)
        {
            if (id > 0)
                this.PrimaryForeigneKeyProperty.SetValueAsId(ressource, id);

            secondaryForeigneKeyProperty = this.FindSecondaryForeignKeyProperty();

            if (secondaryForeigneKeyProperty == null)
                throw new NotImplementedException();

            if (idRelation > 0)
                secondaryForeigneKeyProperty.SetValueAsId(ressource, idRelation);
        }
        #endregion

        [HttpGet]
        public virtual async Task<IActionResult> GetListeRessources(int id)
        {
            return new OkObjectResult(
                await this._datasource.GetListeRessourceAsync<TRessource>(
                 this.GetCriteresListe(id),
                 this._queryService.GetListeFields<TRessource>()));
        }

        [HttpGet("{idRelation}")]
        public virtual async Task<IActionResult> GetRessource(int id, int idRelation)
        {
            TCriteres criteres = this.GetCriteres(id, idRelation);

            if (criteres == null)
                return this.NotFound();

            return new OkObjectResult(
                    await this._datasource.GetRessourceAsync<TRessource>(
                    criteres,
                    this._queryService.GetListeFields<TRessource>()));
        }
    }
}

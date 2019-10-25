using JsonApiArchitectureBase;
using JsonApi.Attributes;
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
    public class JsonApiRelationController<TRessource, TRessourceRelation, TCriteres, TIDataSource, TIRepository, TIRessource, TICriteres> :
        JsonApiRelationReadOnlyController<TRessource, TRessourceRelation, TCriteres, TIDataSource, TIRessource, TICriteres>
           where TRessource : TIRessource, new()
           where TCriteres : TICriteres, new()
           where TIDataSource : IDataSource<TIRessource, TICriteres>
           where TIRepository : IRepository<TIRessource>
    {
        public TIRepository _repository { get; }

        public JsonApiRelationController(TIDataSource dataSource, TIRepository repository, IQueryService queryService) : base(dataSource, queryService)
        {
            this._repository = repository;
        }

        [HttpPatch]
        public virtual async Task<IActionResult> UpdateListeRessource(int id, List<TRessource> ressource)
        {
            // Forcage de l'id sur les ressources par sécurité
            foreach (TRessource res in ressource)
                this.PrimaryForeigneKeyProperty.SetValueAsId(res, id);

            await this._repository.UpdateListeAsync(ressource.Cast<TIRessource>().ToList());

            return await this.GetListeRessources(id);
        }

        [HttpPost]
        [HttpPatch("{idRelation}")]
        public virtual async Task<IActionResult> UpdateRessource(int id, [IdArgumentJsonApi] int idRelation, TRessource ressource)
        {
            new RelationsEngine(this.HttpContext.RequestServices, this._queryService, ressource).SaveRelation();

            try {
                AssignIdsInRessource(ressource, id, idRelation, out PropertyInfo secondaryForeigneKeyProperty);

                int idTemp = await this._repository.UpdateAsync(ressource);

                if (AttributeHandling.GetIdsProperties(typeof(TRessource)).Count() == 1)
                    idRelation = idTemp;
                else
                    idRelation = int.Parse(secondaryForeigneKeyProperty.GetValue(ressource).ToString());

            } catch (NotImplementedException) {
                return NotFound();
            }

            return await this.GetRessource(id, idRelation);
        }

        [HttpDelete("{idRelation}")]
        public virtual async Task<IActionResult> DeleteRessource(int id, int idRelation)
        {
            var ressource = new TRessource();

            try {
                AssignIdsInRessource(ressource, id, idRelation, out PropertyInfo secondaryForeigneKeyProperty);
            } catch (NotImplementedException) {
                return NotFound();
            }

            await this._repository.DeleteAsync(ressource);

            return Ok();
        }
    }
}

using JsonApiArchitectureBase;
using JsonApiArchitectureBase.Interfaces;
using JsonApi.Interface;
using JsonApiArchitecture.Helpers.Application;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace JsonApiArchitecture.Helpers.BaseController
{
    public class JsonApiReadOnlyController<TRessource, TCriteres, TIDataSource, TIRessource, TICriteres> : ControllerBase
           where TRessource : TIRessource, new()
           where TCriteres : ICriteresBase, TICriteres, new()
           where TIDataSource : IDataSource<TIRessource, TICriteres>
    {
        public TIDataSource _datasource { get; }
        public IQueryService _queryService { get; }

        public JsonApiReadOnlyController(TIDataSource dataSource, IQueryService queryService)
        {
            this._datasource = dataSource;
            this._queryService = queryService;
        }

        /// <summary>
        /// Récupere une instance de classe de criteres pour la récupération d'une liste de ressource
        /// </summary>
        protected virtual Task<TCriteres> GetCriteresListe() => Task.FromResult(this._queryService.Filter<TCriteres>());

        [HttpGet]
        public virtual async Task<IActionResult> GetListeRessources()
        {
            return new OkObjectResult(
                await this._datasource.GetListeRessourceAsync<TRessource>(
                 await this.GetCriteresListe(),
                 this._queryService.GetListeFields<TRessource>()));
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetRessource(int id)
        {
            return new OkObjectResult(
                await this._datasource.GetRessourceAsync<TRessource>(
                new TCriteres { Id = id },
                this._queryService.GetListeFields<TRessource>()));
        }

        [HttpGet("schema")]
        public IActionResult GetSchema()
        {
            return new ObjectResult(JsonSchemaJsonApi.GenerateWithJsonApi<TRessource>());
        }
    }
}

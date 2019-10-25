using JsonApiArchitectureBase;
using JsonApiArchitectureBase.Interfaces;
using JsonApi.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace JsonApiArchitecture.Helpers.BaseController
{
    public class JsonApiController<TRessource, TCriteres, TIDataSource, TIRepository, TIRessource, TICriteres> : 
                 JsonApiReadOnlyController<TRessource, TCriteres, TIDataSource, TIRessource, TICriteres>
        where TRessource : TIRessource, new()
        where TCriteres : ICriteresBase, TICriteres, new()
        where TIDataSource : IDataSource<TIRessource, TICriteres>
        where TIRepository : IRepository<TIRessource>
    {

        public TIRepository _repository { get; }

        public JsonApiController(TIDataSource dataSource, TIRepository repository, IQueryService queryService) : base (dataSource, queryService)
        {
            this._repository = repository;
        }

        [HttpPost]
        [HttpPatch("{id}")]
        public virtual async Task<IActionResult> UpdateRessource(int id, TRessource ressource)
        {
            id = await this._repository.UpdateAsync(ressource);
            IActionResult response = await this.GetRessource(id);

            if (this.Request.Method == HttpMethods.Post)
                return new CreatedResult("", (response as ObjectResult).Value);

            return response;
        }

        [HttpDelete("{id}")]
        public virtual async Task DeleteRessource(int id)
        {
            await this._repository.DeleteAsync(id);
        }
    }
}

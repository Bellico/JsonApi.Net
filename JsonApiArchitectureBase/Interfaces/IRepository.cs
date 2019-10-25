using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsonApiArchitectureBase
{
    public interface IRepository<TIRessource>
    {
        Task<int> UpdateAsync(TIRessource ressource);

        Task UpdateListeAsync(List<TIRessource> ressources);

        Task DeleteAsync(int id);

        Task DeleteAsync(TIRessource ressource);
    }
}

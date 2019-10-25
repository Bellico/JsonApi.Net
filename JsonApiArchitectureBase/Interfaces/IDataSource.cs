using JsonApiArchitectureBase.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsonApiArchitectureBase
{
    public interface IDataSource<TIRessource, TICriteres>
    {
        Task<T> GetRessourceAsync<T>(TICriteres criteres, List<string> listeChamps = null) where T : TIRessource, new();
        Task<List<T>> GetListeRessourceAsync<T>(TICriteres criteres, List<string> listeChamps = null) where T : TIRessource, new();
        Task<int> GetCountAsync(TICriteres criteres);
    }
}

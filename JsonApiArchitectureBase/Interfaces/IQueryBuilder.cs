using System.Linq;

namespace JsonApiArchitectureBase
{
   public interface IQueryBuilder<out TEntity>
    {
        IQueryable<TEntity> GetQuery();
    }
}

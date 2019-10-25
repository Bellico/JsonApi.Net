using JsonApiArchitectureBase.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace JsonApiArchitectureBase
{
    public abstract class DataSourceBase<TEntity, TIRessource, TICriteres> : IDataSource<TIRessource, TICriteres>
    {
        protected ListeField<TEntity, TIRessource> ListeField { get; set; }

        protected abstract IQueryBuilder<TEntity> GetQueryBuilder(TICriteres criteres);

        public Task<T> GetRessourceAsync<T>(TICriteres criteres, List<string> listeChamps = null) where T : TIRessource, new()
            => this.SelectFields<T>(this.GetQueryBuilder(criteres).GetQuery(), this.ListeField.FilterField(listeChamps)).FirstOrDefaultAsync();

        public Task<List<T>> GetListeRessourceAsync<T>(TICriteres criteres, List<string> listeChamps = null) where T : TIRessource, new()
            => this.SelectFields<T>(this.GetQueryBuilder(criteres).GetQuery(), this.ListeField.FilterField(listeChamps)).ToListAsync();

        public async Task<int> GetCountAsync(TICriteres criteres)
        {
            Tuple<int, int> skipTake = null;

            // On enleve préalablement le parametre skipTake avant de faire un Count et ainsi avoir un résultat pertinant
            if (criteres is ICriteresBase) {
                skipTake = (criteres as ICriteresBase).SkipTake;
                (criteres as ICriteresBase).SkipTake = null;
            }

            int count = await this.GetQueryBuilder(criteres).GetQuery().CountAsync();

            // Si un parametre skiptake était défini, on le réassigne dans les criteres pour conserver l'intégrité du modele initial
            if (skipTake != null)  (criteres as ICriteresBase).SkipTake = skipTake;
            
            return count;
        }

        private IQueryable<T> SelectFields<T>(IQueryable<TEntity> query, IEnumerable<Field> listeChamps) where T : new()
        {
            Expression<Func<TEntity, T>> selectExpression = new FieldsExpression<TEntity, T>().MakeSelectExpression(listeChamps);

            return query.Select(selectExpression);
        }    
    }
}
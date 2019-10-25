using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace JsonApiArchitectureBase
{
    public abstract class RepositoryBase<TEntity, TIRessource> where TEntity : class, new()
    {
        private readonly FieldsExpression<TEntity, TIRessource> _fieldsExpression;

        protected DbContext _dbContext { get; }
        protected ListeField<TEntity, TIRessource> ListeField { get; set; }

        protected RepositoryBase(DbContext dbContext)
        {
            this._dbContext = dbContext;
            this._fieldsExpression = new FieldsExpression<TEntity, TIRessource>();
        }

        #region IRepository<TIRessource> implémentation
        /// <summary>
        /// Mise à jour d'une ressource
        /// </summary>
        public virtual async Task<int> UpdateAsync(TIRessource ressource)
        {
            TEntity entity = this.CreateOrUpdate(this._dbContext.Set<TEntity>(), ressource);

            await this._dbContext.SaveChangesAsync();

            return GetIdEntity(entity);
        }

        /// <summary>
        /// Mise à jour d'une liste de ressource
        /// </summary>
        public virtual async Task UpdateListeAsync(List<TIRessource> ressources)
        {
            var listeEntity = this._dbContext.Set<TEntity>().Where(entity => ressources.Any(ressource => this.IsMatch(entity, ressource))).ToList();

            foreach (TIRessource ressource in ressources) {
                this.CreateOrUpdate(listeEntity, ressource);
            }

            await this._dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Supprime une ressource par son ou ses identifiants
        /// </summary>
        public async virtual Task DeleteAsync(int id)
        {
            var entity = new TEntity();

            Field fieldId = this.GetFieldsIds().FirstOrDefault();

            string propertyName = ((MemberExpression)fieldId.ExpressionEntity).Member.Name;
            object idValue = fieldId.ExpressionEntity.Type == typeof(decimal) ? (decimal)id : id;
            typeof(TEntity).GetProperty(propertyName).SetValue(entity, idValue);

            this._dbContext.Set<TEntity>().Remove(entity);

            await this._dbContext.SaveChangesAsync();
        }

        public async virtual Task DeleteAsync(TIRessource ressource)
        {
            Expression<Action<TEntity, TIRessource>> expression = this._fieldsExpression.MakeIdAssignementExpression(this.ListeField.Fields);

            var entity = new TEntity();

            expression.Compile().Invoke(entity, ressource);

            this._dbContext.Set<TEntity>().Remove(entity);

            await this._dbContext.SaveChangesAsync();
        }
        #endregion

        #region Helpers virtuals Methodes
        /// <summary>
        /// Ajoute une nouvelle entité ou récupere l'existante en base à partir de son ou ses identifiants
        /// Puis edite ses propriétés grace à la methode EditerEntity
        /// </summary>
        protected TEntity CreateOrUpdate(IEnumerable<TEntity> listeEntity, TIRessource ressource)
        {
            TEntity entity;

            if (this.IsNewRessource(ressource)) {
                entity = this.CreerEntity(ressource);
                this._dbContext.Set<TEntity>().Add(entity);
            } else {
                entity = listeEntity.FirstOrDefault(e => this.IsMatch(e, ressource));
                if (entity == null)
                    throw new ArgumentException($"Aucun element correspondant à la ressource trouvé. Verifier le ou les identifiants de la ressource");
            }

            this.EditerEntity(entity, ressource);

            return entity;
        }

        /// <summary>
        /// Retourne une nouvelle instance de l'entité pour une creation
        /// </summary>
        /// <returns></returns>
        public virtual TEntity CreerEntity(TIRessource ressource) => new TEntity();

        /// <summary>
        /// Est appelée lors de la création / édition pour editer les propriétés de l'entité
        /// </summary>
        public virtual void EditerEntity(TEntity entity, TIRessource ressource)
        {
            Expression<Action<TEntity, TIRessource>> expression = this._fieldsExpression.MakeAssignementExpression(this.ListeField.Fields);

            expression.Compile().Invoke(entity, ressource);
        }

        /// <summary>
        /// Détermine si une entity correspond à une ressource en comparant les identifiants
        /// </summary>
        public virtual bool IsMatch(TEntity entity, TIRessource ressource)
        {
            return this.GetFieldsIds().All(field => {
                BinaryExpression expressionEqual = Expression.Equal(this._fieldsExpression.MakeParamEntity(field.ExpressionEntity), this._fieldsExpression.MakeParamModele(field.ExpressionRessource));
                LambdaExpression lambda = Expression.Lambda(expressionEqual, this._fieldsExpression.ParameterEntity, this._fieldsExpression.ParameterModele);

                return (bool)lambda.Compile().DynamicInvoke(entity, ressource);
            });
        }

        /// <summary>
        /// Détermine si une ressource n'a pas d'identifiant, soit son ou ses identifiants == 0
        /// </summary>
        public virtual bool IsNewRessource(TIRessource ressource)
        {
           if(this.GetFieldsIds().Count() > 1)
                return this._dbContext.Set<TEntity>().FirstOrDefault(e => this.IsMatch(e, ressource)) == null;

            return this.GetFieldsIds().All(field => {
                ConstantExpression constanteZero = field.ExpressionRessource.Type == typeof(decimal) ? Expression.Constant((decimal)0) : Expression.Constant(0);
                LambdaExpression lambda = Expression.Lambda(Expression.Equal(this._fieldsExpression.MakeParamModele(field.ExpressionRessource), constanteZero), this._fieldsExpression.ParameterModele);

                return (bool)lambda.Compile().DynamicInvoke(ressource);
            });
        }
        #endregion

        #region Private Utils
        /// <summary>
        /// Récupère l'identifiant de la clé primaire grace aux expressions
        /// </summary>
        private int GetIdEntity(TEntity entity)
        {
            return this.GetFieldsIds().Select(field => {
               Expression expressionConvert = this._fieldsExpression.MakeParamEntity(Expression.Convert(field.ExpressionEntity, typeof(int)));
                var lambda = Expression.Lambda<Func<TEntity, int>>(expressionConvert, this._fieldsExpression.ParameterEntity);

                return lambda.Compile()(entity);
            }).FirstOrDefault();
        }

        private IEnumerable<Field> GetFieldsIds()
        {
            IEnumerable<Field> fieldsId = this.ListeField.Fields.Where(field => field.Accessibility == FieldAccessibility.FieldId);

            if (!fieldsId.Any())
                throw new ArgumentException($"Le ou les identifiants n'ont pas été précisés dans les champs");

            return fieldsId;
        }
        #endregion
    }
}

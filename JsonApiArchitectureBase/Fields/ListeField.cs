using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JsonApiArchitectureBase
{
    public class ListeField<TEntity, TRessource>
    {
        public List<Field> Fields { get; }

        public ListeField()
        {
            this.Fields = new List<Field>();
        }

        /// <summary>
        /// Définit un champs pour la ressource
        /// Un champs de ressource est toujours récupéré par défaut, sauf si une liste de champs est demandées et qu'il n'y appartient pas
        /// </summary>
        /// <returns></returns>
        public ListeField<TEntity, TRessource> AddField<TResult>(Expression<Func<TRessource, TResult>> expressionRessource, Expression<Func<TEntity, TResult>> expressionEntity, bool canWrite = true)
        {
            this.AddField(expressionRessource, expressionEntity, FieldAccessibility.Field);
            if(!canWrite) this.Fields.Last().CanWrite = false;

            return this;
        }

        /// <summary>
        /// Définit un champs Identifiant
        /// Un champs Identifiant sera toujours récupéré, qu'il soit demandé ou non
        /// </summary>
        /// <returns></returns>
        public ListeField<TEntity, TRessource> AddFieldId<TResult>(Expression<Func<TRessource, TResult>> expressionRessource, Expression<Func<TEntity, TResult>> expressionEntity)
        {
            return this.AddField(expressionRessource, expressionEntity, FieldAccessibility.FieldId);
        }

        /// <summary>
        /// Définit un champs clé pour la ressource
        /// Un champs clé n'est jamais récupéré par défaut, sauf si il est explicitement demandé dans la liste de champs à récuperer
        /// </summary>
        /// <returns></returns>
        public ListeField<TEntity, TRessource> AddFieldKey<TResult>(Expression<Func<TRessource, TResult>> expressionRessource, Expression<Func<TEntity, TResult>> expressionEntity)
        {
            return this.AddField(expressionRessource, expressionEntity, FieldAccessibility.FieldKey);
        }

        private ListeField<TEntity, TRessource> AddField<TResult>(Expression<Func<TRessource, TResult>> expressionRessource, Expression<Func<TEntity, TResult>> expressionEntity, FieldAccessibility accessibility)
        {
            try {
                this.Fields.Add(new Field {
                    PropertyRessourceName = ((MemberExpression)expressionRessource.Body).Member.Name,
                    ExpressionEntity = expressionEntity.Body,
                    ExpressionRessource = expressionRessource.Body,
                    Accessibility = accessibility
                });

                return this;

            } catch (InvalidCastException) {
                throw new ArgumentException("Les retours de fonction ne sont pas compatibles : " + expressionRessource.Body.ToString());
            }
        }

        /// <summary>
        /// Filtre les champs à récuperer
        /// </summary>
        /// <param name="listePropertyName">Liste des champs désirés</param>
        public IEnumerable<Field> FilterField(List<string> listePropertyName)
        {
            // Si des champs spécifique sont demandés
            if (listePropertyName != null) {

                // On filtre les champs demandés qui n'existe pas 
                listePropertyName = listePropertyName.Where(propname => this.Fields.Any(f => f.PropertyRessourceName == propname)).ToList();

                // Si tous les champs demandés sont des champs clés ou Identifiant, alors on veut tous les champs par défaut et les élements demandés
                if (listePropertyName.All(propertyName => this.Fields.Where(f => f.PropertyRessourceName == propertyName).Select(f => f.Accessibility > FieldAccessibility.Field).FirstOrDefault()))
                    return this.Fields.Where(f => listePropertyName.Contains(f.PropertyRessourceName) || f.Accessibility < FieldAccessibility.FieldKey);
                else
                    // Sinon on retourne seulement les champs demandés avec les champs identifiants
                    return this.Fields.Where(f => listePropertyName.Contains(f.PropertyRessourceName) || f.Accessibility == FieldAccessibility.FieldId);
            }

            //Si aucun champs demandés, on retourne seulement les champs par défaut
            return this.Fields.Where(f => f.Accessibility < FieldAccessibility.FieldKey);
        }
    }
}

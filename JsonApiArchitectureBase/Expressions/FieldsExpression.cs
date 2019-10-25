using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JsonApiArchitectureBase
{
    internal class FieldsExpression<TEntity, TModele>
    {
        public ParameterExpression ParameterEntity { get; }
        public ParameterExpression ParameterModele { get; }

        public FieldsExpression()
        {
            this.ParameterEntity = Expression.Parameter(typeof(TEntity));
            this.ParameterModele = Expression.Parameter(typeof(TModele));
        }

        public Expression MakeParamEntity(Expression expression) => ParamExpressionVisitor<TEntity>.MakeParam(this.ParameterEntity, expression);
        public Expression MakeParamModele(Expression expression) => ParamExpressionVisitor<TModele>.MakeParam(this.ParameterModele, expression);

        /// <summary>
        /// Construit l'expression "Select" à partir d'une liste de champs
        /// (m => new Modele {m.p1 = e.P1, m.p2 = e.P2, ..})
        /// </summary>
        public Expression<Func<TEntity, TModele>> MakeSelectExpression(IEnumerable<Field> fields)
        {
            MemberInitExpression body = Expression.MemberInit(
                Expression.New(typeof(TModele)),
                fields
                .Select(f => Expression.Bind(typeof(TModele).GetProperty(f.PropertyRessourceName), this.MakeParamEntity(f.ExpressionEntity)))
                .ToArray()
            );

            return Expression.Lambda<Func<TEntity, TModele>>(body, this.ParameterEntity);
        }

        /// <summary>
        /// Retourne une expression permettant d'assigner les valeurs des identifiants du modele dans celle de l'entité
        /// Soit => TEntity.ID = TModele.Id
        /// ... etc
        /// </summary>
        public Expression<Action<TEntity, TModele>> MakeIdAssignementExpression(IEnumerable<Field> fields)
        {
            IEnumerable<BinaryExpression> assignmentExpressions = fields
                 .Where(field => field.Accessibility == FieldAccessibility.FieldId)
                 .Select(field => Expression.Assign(this.MakeParamEntity(field.ExpressionEntity), this.MakeParamModele(field.ExpressionRessource)));

            return Expression.Lambda<Action<TEntity, TModele>>(Expression.Block(assignmentExpressions), this.ParameterEntity, this.ParameterModele);
        }

        /// <summary>
        /// Retourne un block d'expression d'assignement des propriétés d'un modele vers les propriétés correspondantes d'une entité
        /// La methode "ApplyValue" de la classe "ApplyValueHelper" est appelée avant chaque assignement
        /// Soit => TEntity.PROPERTY1 = ApplyValue(TModele.property1, TEntity.PROPERTY1)
        ///      => TEntity.PROPERTY2 = ApplyValue(TModele.property2, TEntity.PROPERTY2)
        /// ... etc
        /// </summary>
        public Expression<Action<TEntity, TModele>> MakeAssignementExpression(IEnumerable<Field> fields)
        {
            IEnumerable<BinaryExpression> assignmentExpressions = fields
                 .Where(field => field.Accessibility == FieldAccessibility.Field)
                 .Where(field => field.CanWrite)
                 .Where(field => field.ExpressionEntityMemberAccess.NodeType == ExpressionType.MemberAccess)
                 .Select(field => {
                     MethodInfo methodInfo = typeof(ApplyValueHelper).GetMethod(nameof(ApplyValueHelper.ApplyValue), new Type[] { field.ExpressionRessource.Type, field.ExpressionEntityMemberAccess.Type });
                     MethodCallExpression expressionCall = Expression.Call(methodInfo, this.MakeParamModele(field.ExpressionRessource), this.MakeParamEntity(field.ExpressionEntityMemberAccess));

                     // Expression désirée =>  entity.PROPERTY = ApplyValue(ressource.property, entity.PROPERTY)
                     return Expression.Assign(this.MakeParamEntity(field.ExpressionEntityMemberAccess), expressionCall);
                 });

            return Expression.Lambda<Action<TEntity, TModele>>(Expression.Block(assignmentExpressions), this.ParameterEntity, this.ParameterModele);
        }
    }
}

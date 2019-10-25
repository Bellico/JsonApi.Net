using System.Linq.Expressions;

namespace JsonApiArchitectureBase
{
    public class ParamExpressionVisitor<TEntity> : ExpressionVisitor
    {
        private readonly ParameterExpression _param;

        public static Expression MakeParam(ParameterExpression param, Expression body)
        {
            return new ParamExpressionVisitor<TEntity>(param).Visit(body);
        }

        public ParamExpressionVisitor(ParameterExpression param)
        {
            this._param = param;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Type == typeof(TEntity)) {
                return this._param;
            }

            return base.VisitParameter(node);
        }
    }
}

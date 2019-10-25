using System.Linq.Expressions;

namespace JsonApiArchitectureBase
{
    public class Field
    {
        public Field()
        {
            this.CanWrite = true;
        }

        public string PropertyRessourceName { get; set; }
        public Expression ExpressionEntity { get; set; }
        public Expression ExpressionRessource { get; set; }
        public bool CanWrite { get; set; }
        public FieldAccessibility Accessibility { get; set; }

        public Expression ExpressionEntityMemberAccess
        {
            get {
                if (this.ExpressionEntity is UnaryExpression) 
                    return GetExpressionMemberAccess((this.ExpressionEntity as UnaryExpression).Operand);
                
                return GetExpressionMemberAccess(this.ExpressionEntity);
            }
        }

        private Expression GetExpressionMemberAccess(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Equal)
                return (expression as BinaryExpression).Left;

            return expression;
        }
    }

    /// <summary>
    /// Enumeration des champs possible
    /// Conserver l'ordre logique:
    /// 1 = Champs par défaut
    /// 2 = Champs Identifiant
    /// 3 = Champs clé
    /// </summary>
    public enum FieldAccessibility
    {
        // Champs par défaut
        Field = 1,
        // Champs Identifiant
        FieldId = 2,
        // Champs clé
        FieldKey = 3
    }
}

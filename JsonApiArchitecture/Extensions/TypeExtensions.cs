using System;

namespace V_UP.Api.Extensions
{
    public static class TypeExtensions
    {
        public static Type GetPrimaryRessourceType(this Type type)
        {
            // Le parametre généric de la ressource principale est en premiere position
            return type.GenericTypeArguments[0];
        }

        public static Type GetSecondaryRessourceType(this Type type)
        {
            // Le parametre généric de la ressource relation est en deuxième position
            return type.GenericTypeArguments[1];
        }

        public static bool IsControllerRelation(this Type type)
        {
            // Si les deux premiers parametres génériques sont des ressources, on se trouve sur un Controller de relation
            return type.GenericTypeArguments[0].Name.Contains("Ressource") && type.GenericTypeArguments[1].Name.Contains("Ressource");
        }
    }
}

using JsonApi.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace JsonApi.Helpers
{
    /// <summary>
    /// POC Test
    /// </summary>
    public static class FeedObject
    {
        public static object Feed(Type type, int depth = 0)
        {
            if (depth > 1)
                return null;

            // Génération des interfaces
            if (type.GetTypeInfo().IsInterface) return FeedInterface(type, depth);

            // Génération des propriétés simples
            if (Utils.IsTypeSystem(type)) return GetFakeValue(type, depth);

            // Si on ne génère ni une interface ni une propriété simple, on est dans un cas de modèle. On en génère donc une nouvelle instance
            object model = Activator.CreateInstance(type);

            // Cas des List<> de modèle
            if (Utils.IsEnum(model))
            {
                object element = Feed(type.GetGenericArguments()[0], depth + 1);
                if(element != null) ((IList)model).Add(element);
            }
            else
            {
                foreach (PropertyInfo prop in type.GetProperties())
                {
                    try
                    {
                        // On ne traite que les propriétés qui sortiront de l'api, et on évite les boucles infinies en ne générant pas les sous modèles du même type que le parent.
                        if (!IsSameType(prop, type)) FeedProperty(model, prop, depth);
                    }
                    catch (Exception)
                    {
                        // Si jamais une propriété n'arrive pas a se générer, on ne bloque pas la génération du reste du modèle
                    }
                }
            }

            return model;
        }

        private static object FeedInterface(Type type, int depth)
        {
            if (type.GetGenericArguments().Length > 0)
            {
                Type genericType = type.GetGenericArguments()[0];
                object model = Activator.CreateInstance(typeof(List<>).MakeGenericType(genericType));

                ((IList)model).Add(Feed(genericType, depth + 1));

                return model;
            }
            else
            {
                return null;
            }
        }

        private static bool IsSameType(PropertyInfo prop, Type outputType)
        {
            Type inType = prop.PropertyType;
            Type typeprop = AttributeHandling.GetTypeProperty(prop);
            if (prop.PropertyType.GetGenericArguments().Length > 0)
            {
                inType = prop.PropertyType.GetGenericArguments()[0];
                typeprop = AttributeHandling.GetTypeProperty(prop).GetGenericArguments()[0];
            }

            return ((inType == outputType) || (typeprop == outputType));
        }

        private static void BindList(Type type, object model, int depth)
        {
            if (Utils.IsTypeSystem(type))
            {
                throw new Exception();
            }

            object modelItem = Activator.CreateInstance(type);
            BindModel(type, modelItem, depth);
            ((IList)model).Add(modelItem);
        }

        private static void BindModel(Type type, object modelToBind, int depth)
        {
            foreach (PropertyInfo prop in type.GetProperties())
            {
                FeedProperty(modelToBind, prop, depth);
            }
        }

        private static void FeedProperty(object model, PropertyInfo prop, int depth)
        {
            // Récupération du type de la proriété (soit son type direct, soit son type défini dans la propriété "Type" de l'annotation PropertyJsonApi
            Type typeprop = AttributeHandling.GetTypeProperty(prop);
            // Si on est autorisé en écriture (si ce n'est pas une readonly)
            if (prop.CanWrite)
            {
                // La liste ayant un traitement spécifique, on va vérifier qu'il s'agit d'une liste et récupérer immédiatement son type
                Type listType = null;
                if (prop.PropertyType.GetGenericArguments().Length > 0)  listType = prop.PropertyType.GetGenericArguments()[0];

                //Cas de la génération d'une propriété simple
                // /!\ les listes sont des types systèmes. Ainsi les List<Interface> auraient pue passerer dans ce test
                if (Utils.IsTypeSystem(typeprop) && (listType == null || !listType.IsInterface)) prop.SetValue(model, Feed(typeprop, depth + 1));

                // Si on traite une interface, une classe ou un liste d'interface, alors on passera par la génération de modèle
                else if (typeprop.IsInterface || typeprop.IsClass || (listType != null && listType.IsInterface)) FeedPropertyObject(model, prop, depth);
            }
        }

        private static void FeedPropertyObject(object modelToBind, PropertyInfo relationship, int depth)
        {
            // Récupération du type de la propriété
            Type typeRelationship = AttributeHandling.GetTypeProperty(relationship);
            // Génération d'une instance de ce type
            object tempObj = Activator.CreateInstance(typeRelationship);

            // Génération d'une liste de modèle
            if (Utils.IsEnum(tempObj))
            {
                //--------Cas particulier------//
                //Type spécifié
                Type sendedType = typeRelationship.GetGenericArguments()[0];
                //Type attendu 
                Type waitingType = relationship.PropertyType.GetGenericArguments()[0];
                //Instanciation particulière lorsque le type attendu dans le BO est une List<Interface>
                if (waitingType.GetTypeInfo().IsInterface) { tempObj = Activator.CreateInstance(typeof(List<>).MakeGenericType(waitingType)); }
                //---------------------------//

                BindList(sendedType, tempObj, depth);
            }
            // Génération d'un modèle simple
            else
            {
                BindModel(typeRelationship, tempObj, depth);
            }

            try
            {
                relationship.SetValue(modelToBind, tempObj);
            }
            catch (Exception)
            {
                //On ne bloque pas
            }
        }

        private static object GetFakeValue(Type type, int depth)
        {
            if (type.Name.ToLower().Contains("list"))
            {
                // Cas spécifique des propriétés étant de type List(qui est un type système)
                // Dans ce cas, la propriété peut être une liste d'object, d'interface ou de type simple, on repasse donc dans la fonction de Feed au dessus
                Type genericType = type.GetGenericArguments()[0];
                object model = Activator.CreateInstance(typeof(List<>).MakeGenericType(genericType));

                object element = Feed(type.GetGenericArguments()[0], depth + 1);
                if (element != null)
                    ((IList)model).Add(element);

                return model;
            }

            if (type == typeof(string))
            {
                return "string";
            }

            if (type == typeof(int) || type == typeof(int?))
            {
                return 1;
            }

            if (type == typeof(decimal) || type == typeof(decimal?))
            {
                return decimal.Parse("1");
            }

            if (type == typeof(bool) || type == typeof(bool?))
            {
                return true;
            }

            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return DateTime.Now;
            }

            return null;
        }
    }
}

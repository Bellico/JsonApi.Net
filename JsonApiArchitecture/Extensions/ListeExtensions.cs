using System.Collections.Generic;
using System.Linq;

namespace JsonApiArchitecture.Extensions
{
    public static class ListeExtensions
    {
        public static List<int> GetValues(this IEnumerable<int?> liste) => liste.Where(l => l.HasValue).Select(l => l.Value).ToList();

        public static List<int> GetValuesInt(this IEnumerable<int> liste) => liste.Select(l => l).ToList();

        public static List<decimal> GetValues(this IEnumerable<decimal?> liste) => liste.Where(l => l.HasValue).Select(l => l.Value).ToList();

        public static List<int> GetValuesInt(this IEnumerable<decimal> liste) => liste.Select(l => (int)l).ToList();

        public static List<int> ConvertListObjectToListInt(this object liste)
        {
            if (liste is List<int>)
                return liste as List<int>;

            else if (liste is List<int?>)
                return (liste as List<int?>).GetValues().GetValuesInt();

            else if (liste is List<decimal?>)
                return (liste as List<decimal?>).GetValues().GetValuesInt();

            else if (liste is List<decimal>)
                return (liste as List<decimal>).GetValuesInt();

            throw new System.NotImplementedException();
        }
    }
}

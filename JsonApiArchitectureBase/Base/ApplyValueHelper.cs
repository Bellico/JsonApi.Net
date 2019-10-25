using System;

namespace JsonApiArchitectureBase
{
    /// <summary>
    /// Class de logique pour mettre à jour ou non une valeur de base de donnée
    /// </summary>
    public static class ApplyValueHelper
    {
        public static string ApplyValue(string newValue, string oldValue)
        {
            bool nullValue = newValue == "NULL";

            return nullValue ? null : newValue ?? oldValue;
        }

        public static int? ApplyValue(int? newValue, int? oldValue)
        {
            bool nullValue = newValue == -1;

            return nullValue ? null : newValue ?? oldValue;
        }

        public static decimal? ApplyValue(decimal? newValue, decimal? oldValue)
        {
            bool nullValue = newValue == -1;

            return nullValue ? null : newValue ?? oldValue;
        }

        public static DateTime? ApplyValue(DateTime? newValue, DateTime? oldValue)
        {
            bool nullValue = newValue == new DateTime();

            return nullValue ? null : newValue ?? oldValue;
        }

        public static DateTime ApplyValue(DateTime? newValue, DateTime oldValue)
        {
            return newValue ?? oldValue;
        }

        public static bool ApplyValue(bool? newValue, bool oldValue)
        {
            return newValue ?? oldValue;
        }

        public static int ApplyValue(bool? newValue, int oldValue)
        {
            return newValue.HasValue ? (newValue.Value ? 1 : 0) : oldValue;
        }
    }
}

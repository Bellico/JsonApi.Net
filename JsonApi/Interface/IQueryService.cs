using System;
using System.Collections.Generic;

namespace JsonApi.Interface
{
    public interface IQueryService
    {
        string Param(string key, bool isRequired = false);
        int? ParamInt(string key, bool isRequired = false);
        bool? ParamBoolean(string key, bool isRequired = false);
        List<string> ParamList(string key, bool isRequired = false);
        List<decimal> ParamListDecimal(string key, bool isRequired = false);
        T Param<T>() where T : new();
        T Param<T>(string key) where T : new();
        bool HasFilter(string key);
        string Filter(string key, bool isRequired = false);
        List<string> FilterList(string key, bool isRequired = false);
        int? FilterInt(string key, bool isRequired = false);
        bool? FilterBoolean(string key, bool isRequired = false);
        DateTime? FilterDate(string key, bool isRequired = false);
        DateTime? FilterDate(string key, string moment);
        T Filter<T>() where T : new();
        List<T> FilterToList<T>() where T : IQueryParseList, new();
        T FilterOnInclude<T>(string key) where T : new();
        List<Tuple<string, string>> Sorting();
        string GetOrder(string key);
        bool IsAllSelected();
        List<decimal> GetSelectedId();
        List<decimal> GetExcludedId();
        Tuple<int, int> Page(int? default_size);
        Tuple<int, int> PageOffset(int? default_size);
        bool IsInclude<T>();
        bool IsInclude(string relationPath);
        List<string> GetListeFields<T>();
        List<string> GetListeFields(Type type, string relationPath);
        List<string> GetListeFields(string nomType);
    }
}

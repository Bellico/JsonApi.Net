using System;
using System.Collections.Generic;

namespace JsonApiArchitectureBase.Interfaces
{
    public interface ICriteresBase
    {
        int? Id { get; set; }
        List<int> ListeId { get; set; }
        Tuple<int, int> SkipTake { get; set; }
    }
}

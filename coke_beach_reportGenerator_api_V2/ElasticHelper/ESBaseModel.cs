using System;
using System.Collections.Generic;
using System.Text;

namespace coke_beach_reportGenerator_api.ElasticHelper
{
    public class ESBaseModel
    {
        public enum ESLogicOperator
        {
            AND,
            OR
        }

        public class FilterSelection
        {
            public string? ParentName { get; set; }

            public string? Name { get; set; }
        }
    }
}

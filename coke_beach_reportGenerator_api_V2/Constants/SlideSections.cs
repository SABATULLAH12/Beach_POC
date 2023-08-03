using System;
using System.Collections.Generic;
using System.Text;

namespace coke_beach_reportGenerator_api.Constants
{
    public static class SlideSections
    {
        public static readonly List<string> firstSlideSections = new List<string>()
            {
                "Gender",
                "Age",
                "SEC"
            };
        public static readonly List<string> secondSlideSections = new List<string>()
            {
                "Household Size",
                "Kids",
                "Ethnicity"
            };
        public static readonly List<string> secondSlideSectionsNonUS = new List<string>()
            {
                "Household Size",
                "Kids"
            };
        public static Dictionary<string, bool> isReversed = new Dictionary<string, bool>()
        {
            { "Gender",false },
            { "Age",false },
            { "SEC",true },
            { "Household Size",false },
            { "Kids",false },
            { "Ethnicity",false },
        };
        public static readonly List<string> twelfthSlideSections = new List<string>()
            {
                "Awareness",
                "Consumption",
                "Familiarity"
            };
    }
}

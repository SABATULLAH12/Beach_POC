using System;
using System.Collections.Generic;
using System.Text;

namespace coke_beach_reportGenerator_api.Models
{
    public class GeographyLeftPanel
    {
        public string CountryCode { get; set; }
        public int CountryID { get; set; }
        public string Country { get; set; }
        public int RegionID { get; set; }
        public string Region { get; set; }
        public int BottlerID { get; set; }
        public string Bottler { get; set; }
        public int OUID { get; set; }
        public string OU { get; set; }

    }
}

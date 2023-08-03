using System;
using System.Collections.Generic;
using System.Text;

namespace coke_beach_reportGenerator_api.Models
{
    public class constants
    {

        #region start of index names

        public static string check = "check";
        #endregion end of index names



        #region class
        public class Check
        {
            public int Id { get; set; }

            public string CustomerFullName { get; set; }

            public DateTime OrderDate { get; set; }
            public decimal TotalPrice { get; set; }
        }

        #endregion class
    }
}

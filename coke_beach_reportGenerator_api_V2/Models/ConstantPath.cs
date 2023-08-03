using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace coke_beach_reportGenerator_api.Models
{
    public static class ConstantPath
    {
        private static string rootPath = string.Empty;
        public static string GetRootPath { get { return rootPath; } set { rootPath = value; } }
    }
}

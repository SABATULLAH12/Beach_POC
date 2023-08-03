using coke_beach_reportGenerator_api.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace coke_beach_reportGenerator_api.Services.Interfaces
{
    public interface ILeftPanelService
    {
        string connString { get; }
        int timeOut { get; }
        DataSet GetData(string spName, string countryId);
        DataSet GetData(string spName);
        int SaveData(string spName, object[] parameter);
    }
}

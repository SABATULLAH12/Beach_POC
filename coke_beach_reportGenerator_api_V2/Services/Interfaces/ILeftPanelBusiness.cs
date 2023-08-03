using coke_beach_reportGenerator_api.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace coke_beach_reportGenerator_api.Services.Interfaces
{
    public interface ILeftPanelBusiness
    {
        LeftPanel GetLeftPanelData(string countryId);
        LeftPanel GetGeoLeftPanelData();
        LeftPanel SetJsonLeftPanelData();
        DataSet SetJsonMappingForLeftPanel();

    }
}

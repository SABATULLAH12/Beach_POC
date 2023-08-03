using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace coke_beach_reportGenerator_api.Services.Interfaces
{
    public interface ILeftPanelMapping
    {
        DataTable GetGeographyMapping();
        DataTable GetTimeperiodMapping();
        DataTable GetProductMapping();
        DataTable GetFilterMapping();
        DataTable GetSlideMapping();
        void SetLeftPanel(DataSet dset);
        bool CheckLeftPanel();
    }
}

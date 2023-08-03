using coke_beach_reportGenerator_api.Models;
using coke_beach_reportGenerator_api.Models.LeftPanelModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace coke_beach_reportGenerator_api.Services.Interfaces
{
    public interface IReportGeneratorBusiness
    {
        List<SampleSizeModel> CheckSampleSize(LeftPanelRequest leftPanel);
        MemoryStream FormatData(LeftPanelRequest leftPanel);
        MemoryStream FormatDataNew(LeftPanelRequest leftPanel, List<PPTBindingData> demog, List<PPTBindingData> Multi);
    }
}

using coke_beach_reportGenerator_api.Models;
using coke_beach_reportGenerator_api.Models.LeftPanelModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace coke_beach_reportGenerator_api.Services.Interfaces
{
    public interface IReportGeneratorService
    {
        List<SampleSizeModel> CheckSampleSize(DataTable geoTable, long timePeriodId, DataTable benchmarkCompareTable, DataTable demogsTable, DataTable beveragesTable);
        List<PPTBindingData> GetPPTBindingData(string spName, DataTable geoTable, long timePeriodId, DataTable benchmarkCompareTable, DataTable demogsTable, DataTable beveragesTable);
        List<PPTBindingData> GetPPTBindingDataDummy(DataTable geoTable, long timePeriodId, DataTable benchmarkCompareTable, DataTable demogsTable, DataTable beveragesTable);
        List<ImagesList> GetImagesData(DataTable benchmarkCompareTable);
        List<PPTBindingData> GetDuumyPPTBindingData(DataTable geoTable, long timePeriodId, DataTable benchmarkCompareTable, DataTable demogsTable, DataTable beveragesTable);
        PPTResponse GetStatusFromDB(Guid guid);
        void InsertStatusInDB(PPTResponse obj);
        void UpdateStatusInDB(PPTResponse obj);
        void DeleteStatusFromDB(Guid guid);
    }
}

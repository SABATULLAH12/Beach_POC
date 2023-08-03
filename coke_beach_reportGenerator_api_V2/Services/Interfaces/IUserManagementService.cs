using coke_beach_reportGenerator_api.Models.UserManagementModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace coke_beach_reportGenerator_api.Services.Interfaces
{
    public interface IUserManagementService
    {
        List<GetUserManagementModel> GetUserDetails();
        int AddUsers(string Name, string Email, string Location);
        int UpdateUsers(string Email, string Role);
        int AddUserSelectionStat(string Email, string Country, string TimePeriod, string Benchmark, string Comparison, string Filter);
        List<ReportCountModel> GetReportCount(string RoleId, int TimePeriodId);
        List<ReportSelectionDetailModel> GetReportSelectionDetails(string RoleId, int TimePeriodId);
        DataSet GetUSMLeftPanel();
        DataSet GetDataAvailability();
        DataSet GetPBIDashboard();
    }
}

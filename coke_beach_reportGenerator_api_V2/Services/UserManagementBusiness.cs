using coke_beach_reportGenerator_api.Models.UserManagementModels;
using coke_beach_reportGenerator_api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace coke_beach_reportGenerator_api.Services
{
    public class UserManagementBusiness : IUserManagementBusiness
    {
        private readonly IUserManagementService _userManagementService;
        public UserManagementBusiness(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }
        public int AddUsers(string Name, string Email, string Location)
        {
            return _userManagementService.AddUsers(Name, Email, Location);
        }

        public int AddUserSelectionStat(UserManagementRequest userManagementRequest)
        {
            string comparisonSelection = string.Join(",", userManagementRequest.ComparisonMenu.Select(x => x.text.ToString()).ToArray());
            string filterSelection = string.Join(",", userManagementRequest.FilterMenu.Select(x => x.text.ToString()).ToArray());

            return _userManagementService.AddUserSelectionStat(userManagementRequest.EmailId, userManagementRequest.GeographyMenu.text, userManagementRequest.TimeperiodMenu.text, userManagementRequest.BenchmarkMenu.text, comparisonSelection, filterSelection);
        }

        public List<ReportCountModel> GetReportCount(string RoleId, int TimePeriodId)
        {
            return _userManagementService.GetReportCount(RoleId, TimePeriodId);
        }

        public List<ReportSelectionDetailModel> GetReportSelectionDetails(string RoleId, int TimePeriodId)
        {
            return _userManagementService.GetReportSelectionDetails(RoleId, TimePeriodId);
        }

        public List<GetUserManagementModel> GetUserDetails()
        {
            return _userManagementService.GetUserDetails(); ;
        }

        public DataSet GetUSMLeftPanel()
        {
            return _userManagementService.GetUSMLeftPanel();
        }

        public int UpdateUsers(string Email, string Role)
        {
            return _userManagementService.UpdateUsers(Email, Role);
        }

        public DataSet GetDataAvailability()
        {
            return _userManagementService.GetDataAvailability();
        }

        public DataSet GetPBIDashboard()
        {
            return _userManagementService.GetPBIDashboard();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace coke_beach_reportGenerator_api.Models.UserManagementModels
{
    public class UserManagementModel
    {
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string Location { get; set; }
    }
    public class GetUserManagementModel
    {
        public int SrNo { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string Date { get; set; }
        public string Role { get; set; }
    }
    public class UpdateUserModel
    {
        public string EmailId { get; set; }
        public string Role { get; set; }
    }
    public class UserManagementRequest
    {
        public string EmailId { get; set; }
        public PopupLevelData GeographyMenu { get; set; }
        public TimePeriod TimeperiodMenu { get; set; }
        public PopupLevelData BenchmarkMenu { get; set; }
        public List<PopupLevelData> ComparisonMenu { get; set; }
        public List<PopupLevelData> FilterMenu { get; set; }
    }
    public class ReportCountModel
    {
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string Role { get; set; }
        public int ReportDownloadCount { get; set; }
    }
    public class ReportSelectionDetailModel
    {
        public string SrNo { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string Role { get; set; }
        public string DownloadDate { get; set; }
        public string Geography { get; set; }
        public string TimePeriod { get; set; }
        public string Benchmark { get; set; }
        public string Comparison { get; set; }
        public string Filter { get; set; }
    }
}

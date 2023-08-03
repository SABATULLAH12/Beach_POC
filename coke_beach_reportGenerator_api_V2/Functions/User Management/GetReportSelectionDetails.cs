using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using coke_beach_reportGenerator_api.Models.UserManagementModels;
using coke_beach_reportGenerator_api.Services.Interfaces;

namespace coke_beach_reportGenerator_api.Functions.User_Management
{
    public class GetReportSelectionDetails
    {
        private readonly IUserManagementBusiness _userManagementBusiness;
        public GetReportSelectionDetails(IUserManagementBusiness userManagementBusiness)
        {
            _userManagementBusiness = userManagementBusiness;
        }
        [FunctionName("GetReportSelectionDetails")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<ReportSelectionDetailModel> result = new List<ReportSelectionDetailModel>();
            try
            {
                string RoleId = req.Query["RoleId"];
                int TimePeriodId = Convert.ToInt32(req.Query["TimePeriodId"]);

                result = _userManagementBusiness.GetReportSelectionDetails(RoleId, TimePeriodId);
            }
            catch (Exception e)
            {
                log.LogError(e.Message.ToString());
            }
            return new OkObjectResult(result);
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using coke_beach_reportGenerator_api.Services.Interfaces;
using coke_beach_reportGenerator_api.Models.UserManagementModels;
using System.Collections.Generic;

namespace coke_beach_reportGenerator_api.Functions.User_Management
{
    public class GetReportCount
    {
        private readonly IUserManagementBusiness _userManagementBusiness;
        public GetReportCount(IUserManagementBusiness userManagementBusiness)
        {
            _userManagementBusiness = userManagementBusiness;
        }
        [FunctionName("GetReportCount")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<ReportCountModel> result = new List<ReportCountModel>();
            try
            {
                string RoleId = req.Query["RoleId"];
                int TimePeriodId = Convert.ToInt32(req.Query["TimePeriodId"]);

                result = _userManagementBusiness.GetReportCount(RoleId, TimePeriodId);
            }
            catch (Exception e)
            {
                log.LogError(e.Message.ToString());
            }
            return new OkObjectResult(result);
        }
    }
}

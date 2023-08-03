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
using coke_beach_reportGenerator_api.Helper;

namespace coke_beach_reportGenerator_api.Functions.User_Management
{
    public class AddUserSelectionStats
    {
        private readonly IUserManagementBusiness _userManagementBusiness;
        public AddUserSelectionStats(IUserManagementBusiness userManagementBusiness)
        {
            _userManagementBusiness = userManagementBusiness;
        }
        [FunctionName("AddUserSelectionStats")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var data = await req.GetBodyAsync<UserManagementRequest>();
            int rowCount = 0;
            try
            {
                rowCount = _userManagementBusiness.AddUserSelectionStat(data.Value);
            }
            catch (Exception e)
            {
                log.LogError(e.Message.ToString());
            }
            if (rowCount == -1)
            {
                return new NotFoundObjectResult("Email Id is not present");
            }
            return new OkObjectResult(rowCount);
        }
    }
}

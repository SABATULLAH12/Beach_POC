using coke_beach_reportGenerator_api.Models.UserManagementModels;
using coke_beach_reportGenerator_api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace coke_beach_reportGenerator_api.Functions.User_Management
{
    public class GetUserDetails
    {
        private readonly IUserManagementBusiness _userManagementBusiness;
        public GetUserDetails(IUserManagementBusiness userManagementBusiness)
        {
            _userManagementBusiness = userManagementBusiness;
        }
        [FunctionName("GetUserDetails")]
        public async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            List<GetUserManagementModel> userDetails = new List<GetUserManagementModel>();
            try
            {
                userDetails = _userManagementBusiness.GetUserDetails();
            }
            catch (Exception e)
            {
                log.LogError(e.Message.ToString());
            }
            return new OkObjectResult(userDetails);
        }
    }
}

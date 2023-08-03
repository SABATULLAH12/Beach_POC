using coke_beach_reportGenerator_api.Helper;
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
    public class UpdateUserData
    {
        private readonly IUserManagementBusiness _userManagementBusiness;
        public UpdateUserData(IUserManagementBusiness userManagementBusiness)
        {
            _userManagementBusiness = userManagementBusiness;
        }
        [FunctionName("UpdateUserRole")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req, ILogger log)
        {
            var data = await req.GetBodyAsync<UpdateUserModel>();
            int rowCount = 0;
            try
            {
                string Email = data.Value.EmailId.ToString();
                string Role = data.Value.Role.ToString();

                rowCount = _userManagementBusiness.UpdateUsers(Email, Role);
            }
            catch (Exception e)
            {
                log.LogError(e.Message.ToString());
            }
            return new OkObjectResult(rowCount);
        }
    }
}

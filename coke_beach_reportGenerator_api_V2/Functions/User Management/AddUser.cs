using coke_beach_reportGenerator_api.Helper;
using coke_beach_reportGenerator_api.Models.UserManagementModels;
using coke_beach_reportGenerator_api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
    public class AddUser
    {
        private readonly IUserManagementBusiness _userManagementBusiness;
        public AddUser(IUserManagementBusiness userManagementBusiness)
        {
            _userManagementBusiness = userManagementBusiness;
        }
        [FunctionName("AddUser")]
        public async Task<IActionResult> Run(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            var data = await req.GetBodyAsync<UserManagementModel>();
            int rowCount = 0;
            try
            {
                string Name = data.Value.UserName.ToString();
                string Email = data.Value.EmailId.ToString();
                //string Location = data.Value.Location.ToString();
                string Location = "";

                // rowCount = _userManagementBusiness.AddUsers(Name, Email, Location);
                rowCount = 1;
            }
            catch (Exception e)
            {
                log.LogError(e.Message.ToString());
            }
            return new OkObjectResult(rowCount);
        }
    }
}

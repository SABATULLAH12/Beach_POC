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
using System.Data;

namespace coke_beach_reportGenerator_api.Functions.User_Management
{
    public class GetUSMLeftPanel
    {
        private readonly IUserManagementBusiness _userManagementBusiness;
        public GetUSMLeftPanel(IUserManagementBusiness userManagementBusiness)
        {
            _userManagementBusiness = userManagementBusiness;
        }
        [FunctionName("GetUSMLeftPanel")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            DataSet data = new DataSet();
            try
            {
                data = _userManagementBusiness.GetUSMLeftPanel();
            }
            catch (Exception e)
            {
                log.LogError(e.Message.ToString());
            }
            return new OkObjectResult(data);
        }
    }
}

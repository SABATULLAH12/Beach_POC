using coke_beach_reportGenerator_api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace coke_beach_reportGenerator_api.Functions
{
    class GetDataAvailability
    {
        private readonly IUserManagementBusiness _userManagementBusiness;
        public GetDataAvailability(IUserManagementBusiness userManagementBusiness)
        {
            _userManagementBusiness = userManagementBusiness;
        }
        [FunctionName("GetDataAvailability")]
        public async Task<IActionResult> Run(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {

            DataSet data = new DataSet();
            try
            {
                data = _userManagementBusiness.GetDataAvailability();
            }
            catch (Exception e)
            {
                log.LogError(e.Message.ToString());
            }
            return new OkObjectResult(data);
        }
    }
}

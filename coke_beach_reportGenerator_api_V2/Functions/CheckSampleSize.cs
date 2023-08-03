using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using coke_beach_reportGenerator_api.Helper;
using coke_beach_reportGenerator_api.Models.LeftPanelModel;
using coke_beach_reportGenerator_api.Services.Interfaces;
using System.Collections.Generic;
using coke_beach_reportGenerator_api.Models;

namespace coke_beach_reportGenerator_api.Functions
{
    public class CheckSampleSize
    {
        private readonly IReportGeneratorBusiness _reportGeneratorBusiness;
        public CheckSampleSize(IReportGeneratorBusiness reportGeneratorBusiness)
        {
            _reportGeneratorBusiness = reportGeneratorBusiness;
        }
        [FunctionName("CheckSampleSize")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var data = await req.GetBodyAsync<LeftPanelRequest>();
            
            var sampleSizesList = _reportGeneratorBusiness.CheckSampleSize(data.Value);

            return new OkObjectResult(sampleSizesList);
        }
    }
}

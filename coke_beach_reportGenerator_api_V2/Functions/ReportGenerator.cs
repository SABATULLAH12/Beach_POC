using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using coke_beach_reportGenerator_api.Models.LeftPanelModel;
using coke_beach_reportGenerator_api.Helper;
using coke_beach_reportGenerator_api.Services.Interfaces;
using System.Collections.Generic;
using System.Threading;

namespace coke_beach_reportGenerator_api.Functions
{
    public class ReportGenerator
    {
        private readonly IReportGeneratorBusiness _reportGeneratorBusiness;
        private readonly IReportGeneratorService _reportGeneratorService;
        private static Dictionary<Guid, PPTResponse> runningTasks = new Dictionary<Guid, PPTResponse>();
        public ReportGenerator(IReportGeneratorBusiness reportGeneratorBusiness,IReportGeneratorService service)
        {
            _reportGeneratorBusiness = reportGeneratorBusiness;
            _reportGeneratorService = service;
        }

        [FunctionName("ReportGenerator")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ReportGenerator")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            Guid id;
            var obj = new Object();
            lock(obj){
                 id= Guid.NewGuid();  //Generate tracking Id
            }

            PPTResponse response = new PPTResponse() { isCompleted = false, isError = false };

            response.id = id;

            _reportGeneratorService.InsertStatusInDB(response);

            runningTasks.Add(id, response);  //Job isn't done yet

            var data = await req.GetBodyAsync<LeftPanelRequest>();

            new Thread(() => DownloadPPT(data.Value, id, log)).Start();

            return new OkObjectResult(response);
        }

        public void DownloadPPT(LeftPanelRequest data, Guid id, ILogger log)
        {
            MemoryStream ms = null;
            try
            {
                ms = _reportGeneratorBusiness.FormatData(data);
                byte[] bytes = ms.ToArray();
                ms.Dispose();


                runningTasks[id].isCompleted = true;
                runningTasks[id].data = bytes;

                _reportGeneratorService.UpdateStatusInDB(runningTasks[id]);
                
                
                
            }
            catch (Exception e)
            {
                runningTasks[id].isError = true;
                runningTasks[id].errorMessage = e.Message;
                _reportGeneratorService.UpdateStatusInDB(runningTasks[id]);
            }
        }

        [FunctionName("PollStatus")]
        public async Task<IActionResult> Run1([HttpTrigger(AuthorizationLevel.Function, "post", Route = "PollStatus")] HttpRequest req,
            ILogger log)
        {
            var res = (await req.GetBodyAsync<PPTResponse>()).Value;
            var id = res.id;
            PPTResponse response = new PPTResponse();

            //If the job is completed
            if (runningTasks.ContainsKey(id) && (runningTasks[id].isCompleted || runningTasks[id].isError))
            {
                response.isCompleted = runningTasks[id].isCompleted;
                response.errorMessage = runningTasks[id].isError ? "Some error Occurred" : "";
                response.isError = runningTasks[id].isError;
                response.id = id;
                if (runningTasks[id].isError)
                {
                    runningTasks.Remove(runningTasks[id].id);
                    _reportGeneratorService.DeleteStatusFromDB(id);
                }
                return new OkObjectResult(response);
            }

            //If the job is still running
            else if (runningTasks.ContainsKey(id))
            {
                response.isCompleted = false;
                response.errorMessage = "";
                response.isError = false;
                response.id = id;
                return new OkObjectResult(response);
            }
            else
            {
                PPTResponse responseFromDB = null;
                try
                {
                    responseFromDB = _reportGeneratorService.GetStatusFromDB(res.id);
                }
                catch(Exception e)
                {
                    responseFromDB = null;
                }
                   
                if (responseFromDB == null)
                {
                    response.isCompleted = false;
                    response.errorMessage = "Job doesnot exist";
                    response.isError = true;
                    response.id = res.id;
                }
                else
                {
                    runningTasks.Add(res.id, responseFromDB);
                }
                return new OkObjectResult(response);
            }
        }

        [FunctionName("Download")]
        public async Task<IActionResult> Run2([HttpTrigger(AuthorizationLevel.Function, "post", Route = "Download")] HttpRequest req,
            ILogger log)
        {
            var res = (await req.GetBodyAsync<PPTResponse>()).Value;
            var id = res.id;

            

            byte[] bytes = runningTasks[id].data;
            runningTasks.Remove(id);
            _reportGeneratorService.DeleteStatusFromDB(id);
            return new FileContentResult(bytes, "application/vnd.openxmlformats-officedocument.presentationml.presentation")
            {
                FileDownloadName = "Output.pptx"
            };
        }
    }
}

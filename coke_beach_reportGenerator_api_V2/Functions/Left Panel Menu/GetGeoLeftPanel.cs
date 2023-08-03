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
using coke_beach_reportGenerator_api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using Azure.Storage.Blobs;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using Azure.Storage.Blobs.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Data;

namespace coke_beach_reportGenerator_api.Functions.POC
{
    public class GetGeoLeftPanel
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILeftPanelBusiness _leftPanelBusiness;
        private LeftPanel _leftPanelData = null;
        private ILeftPanelMapping _leftpanelMapping;
        public GetGeoLeftPanel(ILeftPanelBusiness leftPanelBusiness, ILeftPanelMapping leftPanelMapping, IHostingEnvironment hostingEnvironment)
        {
            _leftPanelBusiness = leftPanelBusiness;
            _hostingEnvironment = hostingEnvironment;
            _leftpanelMapping = leftPanelMapping;
        }
        [FunctionName("GetGeoLeftPanel")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetGeoLeftPanel")] HttpRequest req,
            ILogger log)
        {
            LeftPanel leftPanel = new LeftPanel();
            log.LogInformation("Db Called");
            var file = ConstantPath.GetRootPath + @"Json\" + "leftPanel.json";
            using (StreamReader r = new StreamReader(file))
            {
                string data = r.ReadToEnd();
                LeftPanel _leftPanel = JsonConvert.DeserializeObject<LeftPanel>(data);
                leftPanel.geographyMenu = _leftPanel.geographyMenu;
                r.Close();
            }

            if (!_leftpanelMapping.CheckLeftPanel())
            {
                var geoFile = ConstantPath.GetRootPath + @"Json\" + "geographyMapping.json";
                DataTable geoTable = null;
                using (StreamReader r = new StreamReader(geoFile))
                {
                    string data = r.ReadToEnd();
                    geoTable = JsonConvert.DeserializeObject<DataTable>(data);
                    r.Close();
                }
                var timeFile = ConstantPath.GetRootPath + @"Json\" + "timeperiodMapping.json";
                DataTable timeTable = null;
                using (StreamReader r = new StreamReader(timeFile))
                {
                    string data = r.ReadToEnd();
                    timeTable = JsonConvert.DeserializeObject<DataTable>(data);
                    r.Close();
                }
                var prodFile = ConstantPath.GetRootPath + @"Json\" + "productMapping.json";
                DataTable prodTable = null;
                using (StreamReader r = new StreamReader(prodFile))
                {
                    string data = r.ReadToEnd();
                    prodTable = JsonConvert.DeserializeObject<DataTable>(data);
                    r.Close();
                }
                var filterFile = ConstantPath.GetRootPath + @"Json\" + "attributeMapping.json";
                DataTable filterTable = null;
                using (StreamReader r = new StreamReader(filterFile))
                {
                    string data = r.ReadToEnd();
                    filterTable = JsonConvert.DeserializeObject<DataTable>(data);
                    r.Close();
                }
                var slideFile = ConstantPath.GetRootPath + @"Json\" + "SlideMapping.json";
                DataTable slideTable = null;
                using (StreamReader r = new StreamReader(slideFile))
                {
                    string data = r.ReadToEnd();
                    slideTable = JsonConvert.DeserializeObject<DataTable>(data);
                    r.Close();
                }
                DataSet dset = new DataSet();
                dset.Tables.Add(geoTable);
                dset.Tables.Add(timeTable);
                dset.Tables.Add(prodTable);
                dset.Tables.Add(filterTable);
                dset.Tables.Add(slideTable);
                _leftpanelMapping.SetLeftPanel(dset);
            }
            /*if (_leftPanelData == null)
            {
                _leftPanelData = _leftPanelBusiness.GetGeoLeftPanelData();
                leftPanel = _leftPanelData;
            }
            else
            {
                leftPanel = _leftPanelData;
            }*/
            var obj = JsonConvert.SerializeObject(leftPanel);
            return new OkObjectResult(leftPanel);
        }
        [FunctionName("GetLeftPanelJson")]
        public async Task<IActionResult> Run1(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetLeftPanelJson")] HttpRequest req,
            ILogger log)
        {
            LeftPanel leftPanel = new LeftPanel();
            log.LogInformation("Db Called");
            LeftPanel _leftPanelData = null;
            
            _leftPanelData = _leftPanelBusiness.SetJsonLeftPanelData();
            var file = ConstantPath.GetRootPath + @"Json\" + "leftPanel.json";
            if (!CheckFileExists(file))
            {
                using (var tw = new StreamWriter(file, true))
                {
                    string jResult = JsonConvert.SerializeObject(_leftPanelData);
                    tw.Write(jResult);
                    tw.Close();
                }
            }
            
            //_leftPanelData = _leftPanelBusiness.GetGeoLeftPanelData();
            
            return new OkObjectResult("File Created");
        }
        [FunctionName("GetJsonMappingLeftPanel")]
        public async Task<IActionResult> Run2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetJsonMappingLeftPanel")] HttpRequest req,
            ILogger log)
        {
            
            log.LogInformation("Db Called");
            DataSet _leftPanelData = null;

            _leftPanelData = _leftPanelBusiness.SetJsonMappingForLeftPanel();
            DataTable productMapping = _leftPanelData.Tables[0];
            DataTable timeperiodMapping = _leftPanelData.Tables[1];
            DataTable attributeMapping = _leftPanelData.Tables[2];
            DataTable geographyMapping = _leftPanelData.Tables[3];
            DataTable slideMapping = _leftPanelData.Tables[4];
            var file = ConstantPath.GetRootPath + @"Json\" + "productMapping.json";
            if (!CheckFileExists(file))
            {
                using (var tw = new StreamWriter(file, true))
                {
                    string jResult = JsonConvert.SerializeObject(productMapping);
                    tw.Write(jResult);
                    tw.Close();
                }
            }
            file = ConstantPath.GetRootPath + @"Json\" + "timeperiodMapping.json";
            if (!CheckFileExists(file))
            {
                using (var tw = new StreamWriter(file, true))
                {
                    string jResult = JsonConvert.SerializeObject(timeperiodMapping);
                    tw.Write(jResult);
                    tw.Close();
                }
            }
            file = ConstantPath.GetRootPath + @"Json\" + "attributeMapping.json";
            if (!CheckFileExists(file))
            {
                using (var tw = new StreamWriter(file, true))
                {
                    string jResult = JsonConvert.SerializeObject(attributeMapping);
                    tw.Write(jResult);
                    tw.Close();
                }
            }
            file = ConstantPath.GetRootPath + @"Json\" + "geographyMapping.json";
            if (!CheckFileExists(file))
            {
                using (var tw = new StreamWriter(file, true))
                {
                    string jResult = JsonConvert.SerializeObject(geographyMapping);
                    tw.Write(jResult);
                    tw.Close();
                }
            }
            file = ConstantPath.GetRootPath + @"Json\" + "SlideMapping.json";
            if (!CheckFileExists(file))
            {
                using (var tw = new StreamWriter(file, true))
                {
                    string jResult = JsonConvert.SerializeObject(slideMapping);
                    tw.Write(jResult);
                    tw.Close();
                }
            }
            //_leftPanelData = _leftPanelBusiness.GetGeoLeftPanelData();

            return new OkObjectResult("File Created");
        }
        public DataConfiguration GetConfiguration()
        {
            DataConfiguration _config = new DataConfiguration();
            _config.ConnectionString = this._configuration.GetConnectionString("BeachDBConnection");
            _config.TimeOut = this._configuration.GetValue<int>("TimeOut");
            return _config;
        }
        public string LeftpanelJsonName()
        {
            return Path.Combine("json", "LeftPanel.json");
        }
        public bool CheckFileExists(string path)
        {
            return System.IO.File.Exists(path);
        }
        private static MemoryStream SerializeToStream(object o)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream;
        }
    }
}

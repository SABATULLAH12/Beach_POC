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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using coke_beach_reportGenerator_api.Models.LeftPanelModel;
using coke_beach_reportGenerator_api.Helper;
using System.Linq;
using System.Collections.Generic;

namespace coke_beach_reportGenerator_api.Functions.POC
{
    public class GetBenchmarkLeftPanel
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILeftPanelBusiness _leftPanelBusiness;
        private LeftPanel _leftPanelData = null;
        public GetBenchmarkLeftPanel(ILeftPanelBusiness leftPanelBusiness, IHostingEnvironment hostingEnvironment)
        {
            _leftPanelBusiness = leftPanelBusiness;
            _hostingEnvironment = hostingEnvironment;
        }
        [FunctionName("GetBenchmarkLeftPanel")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "GetBenchmarkLeftPanel")] HttpRequest req,
            ILogger log)
        {
            LeftPanel leftPanel = new LeftPanel();
            log.LogInformation("Db Called");
            
            var data = await req.GetBodyAsync<BenchmarkPostModel>();
            log.LogInformation(data.Value.CountryId);
            var file = ConstantPath.GetRootPath + @"Json\" + "leftPanel.json";
            using (StreamReader r = new StreamReader(file))
            {
                string tempdata = r.ReadToEnd();
                LeftPanel _leftPanel = JsonConvert.DeserializeObject<LeftPanel>(tempdata);

                leftPanel = filterData(_leftPanel, data.Value.CountryId);
                r.Close();
            }
            /*if (_leftPanelData == null)
            {
               // _leftPanelData = _leftPanelBusiness.GetLeftPanelData(data.Value.CountryId);
                
                leftPanel = _leftPanelData;
            }
            else
            {
                leftPanel = _leftPanelData;
            }
            var obj = JsonConvert.SerializeObject(leftPanel);*/
            return new OkObjectResult(leftPanel);
        }
        public LeftPanel filterData(LeftPanel leftPanel,string countryId)
        {
            LeftPanel _leftPanel = new LeftPanel();

            _leftPanel.timeperiodMenu = leftPanel.timeperiodMenu.Where(a => a.geography == countryId).ToList();

            _leftPanel.benchmarkMenu = new LeftPanelMenu();
            _leftPanel.benchmarkMenu.id = leftPanel.benchmarkMenu.id;
            _leftPanel.benchmarkMenu.name = leftPanel.benchmarkMenu.name;
            _leftPanel.benchmarkMenu.data = new List<PopupLevel>();
            foreach(var level in leftPanel.benchmarkMenu.data)
            {
                PopupLevel _level = new PopupLevel();
                _level.levelId = level.levelId;
                _level.levelName = level.levelName;
                _level.data =  level.data.Where(e => e.GeographyId == countryId).ToList();
                _leftPanel.benchmarkMenu.data.Add(_level);
            }

            _leftPanel.comparisonMenu = new LeftPanelMenu();
            _leftPanel.comparisonMenu.id = leftPanel.comparisonMenu.id;
            _leftPanel.comparisonMenu.name = leftPanel.comparisonMenu.name;
            _leftPanel.comparisonMenu.data = new List<PopupLevel>();
            foreach (var level in leftPanel.comparisonMenu.data)
            {
                PopupLevel _level = new PopupLevel();
                _level.levelId = level.levelId;
                _level.levelName = level.levelName;
                _level.data = level.data.Where(e => e.GeographyId == countryId).ToList();
                _leftPanel.comparisonMenu.data.Add(_level);
            }

            _leftPanel.filterMenu = new LeftPanelMenu();
            _leftPanel.filterMenu.id = leftPanel.filterMenu.id;
            _leftPanel.filterMenu.name = leftPanel.filterMenu.name;
            _leftPanel.filterMenu.data = new List<PopupLevel>();
            foreach (var level in leftPanel.filterMenu.data)
            {
                PopupLevel _level = new PopupLevel();
                _level.levelId = level.levelId;
                _level.levelName = level.levelName;
                _level.data = level.data.Where(e => (level.levelId>2 && e.GeographyId == countryId)||  (e.type=="BeverageFilter" && level.levelId==2 && e.GeographyId == countryId) || (e.type== "Filter" && level.levelId == 2)  || level.levelId==1).ToList();
                _leftPanel.filterMenu.data.Add(_level);
            }
            return _leftPanel;
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

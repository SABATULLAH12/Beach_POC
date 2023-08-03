using coke_beach_reportGenerator_api.Models;
using coke_beach_reportGenerator_api.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using coke_beach_reportGenerator_api.Models.LeftPanelModel;
using System.Data;
using System;
using Aspose.Slides;
using Aspose.Slides.Export;
using System.Linq;
using Aspose.Slides.Charts;
using System.Drawing;
using coke_beach_reportGenerator_api.Constants;
using Azure.Storage.Blobs;
using System.Globalization;
using coke_beach_reportGenerator_api.Helper;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace coke_beach_reportGenerator_api.Services
{
    public class ReportGeneratorBusiness : IReportGeneratorBusiness
    {
        private readonly IReportGeneratorService _reportGeneratorService;
        private readonly ILeftPanelMapping _leftpanelMapping;
        private IConfiguration _iConfig;
        // DI using constructor:
        public ReportGeneratorBusiness(IReportGeneratorService reportGeneratorService, ILeftPanelMapping leftPanelMapping, IConfiguration iConfig)
        {
            _reportGeneratorService = reportGeneratorService;
            _leftpanelMapping = leftPanelMapping;
            _iConfig = iConfig;
            if (!_leftpanelMapping.CheckLeftPanel())
            {
                var geoFile = ConstantPath.GetRootPath + @"Json\" + "geographyMapping.json";
                System.Data.DataTable geoTable = null;
                using (StreamReader r = new StreamReader(geoFile))
                {
                    string data = r.ReadToEnd();
                    geoTable = JsonConvert.DeserializeObject<System.Data.DataTable>(data);
                    r.Close();
                }
                var timeFile = ConstantPath.GetRootPath + @"Json\" + "timeperiodMapping.json";
                System.Data.DataTable timeTable = null;
                using (StreamReader r = new StreamReader(timeFile))
                {
                    string data = r.ReadToEnd();
                    timeTable = JsonConvert.DeserializeObject<System.Data.DataTable>(data);
                    r.Close();
                }
                var prodFile = ConstantPath.GetRootPath + @"Json\" + "productMapping.json";
                System.Data.DataTable prodTable = null;
                using (StreamReader r = new StreamReader(prodFile))
                {
                    string data = r.ReadToEnd();
                    prodTable = JsonConvert.DeserializeObject<System.Data.DataTable>(data);
                    r.Close();
                }
                var filterFile = ConstantPath.GetRootPath + @"Json\" + "attributeMapping.json";
                System.Data.DataTable filterTable = null;
                using (StreamReader r = new StreamReader(filterFile))
                {
                    string data = r.ReadToEnd();
                    filterTable = JsonConvert.DeserializeObject<System.Data.DataTable>(data);
                    r.Close();
                }
                var slideFile = ConstantPath.GetRootPath + @"Json\" + "SlideMapping.json";
                System.Data.DataTable slideTable = null;
                using (StreamReader r = new StreamReader(slideFile))
                {
                    string data = r.ReadToEnd();
                    slideTable = JsonConvert.DeserializeObject<System.Data.DataTable>(data);
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
        }
        // Method for checking sample size:
        public List<SampleSizeModel> CheckSampleSize(LeftPanelRequest leftPanel)
        {
            List<PopupLevelData> geoPopupLevels = new List<PopupLevelData>();
            geoPopupLevels.Add(leftPanel.GeographyMenu);

            System.Data.DataTable geographyTable = GetTableOutputPPT(geoPopupLevels, "GeographyMenu", new System.Data.DataTable());

            List<PopupLevelData> benchamrkPopupLevels = new List<PopupLevelData>();
            benchamrkPopupLevels.Add(leftPanel.BenchmarkMenu);
            System.Data.DataTable benchmarkTable = GetTableOutputPPT(benchamrkPopupLevels, "Benchmark", new System.Data.DataTable());

            System.Data.DataTable comparisonTable = GetTableOutputPPT(leftPanel.ComparisonMenu, "Comparison", new System.Data.DataTable());

            System.Data.DataTable demogsTable = GetTableOutputPPT(leftPanel.FilterMenu, "FilterMenu", new System.Data.DataTable(), "Filter");

            System.Data.DataTable beveragesTable = GetTableOutputPPT(leftPanel.FilterMenu, "FilterMenu", new System.Data.DataTable(), "BeverageFilter");

            benchmarkTable.Merge(comparisonTable);
            List<SampleSizeModel> sampleSizeList = _reportGeneratorService.CheckSampleSize(geographyTable, leftPanel.TimeperiodMenu.id, benchmarkTable, demogsTable, beveragesTable);

            foreach (var el in sampleSizeList)
            {

                el.DetailedText = el.SelectionName + " ";
                switch (el.ConsumptionId)
                {
                    case 1: el.DetailedText += "- Observed Drinkers"; break;
                    case 2: el.DetailedText += "- Daily"; break;
                    case 3: el.DetailedText += "- Weekly"; break;
                    case 4: el.DetailedText += "- Weekly+"; break;
                    case 5: el.DetailedText += "- Monthly"; break;
                    case 6: el.DetailedText += "- Occasional"; break;
                    default: el.DetailedText += "- Observed Drinkers"; break;
                }
            }

            // Filter out the data which has sample size less than 30
            var newList = sampleSizeList.Where(x => x.SampleSize < 30).ToList();

            return newList;
        }
        // Master method for generating output file:
        public MemoryStream FormatData(LeftPanelRequest leftPanel)
        {
            string FileName = string.Empty;
            List<PopupLevelData> geoPopupLevels = new List<PopupLevelData>();
            geoPopupLevels.Add(leftPanel.GeographyMenu);

            System.Data.DataTable geographyTable = GetTableOutputPPT(geoPopupLevels, "GeographyMenu", new System.Data.DataTable());

            List<PopupLevelData> benchamrkPopupLevels = new List<PopupLevelData>();
            benchamrkPopupLevels.Add(leftPanel.BenchmarkMenu);
            System.Data.DataTable benchmarkTable = GetTableOutputPPT(benchamrkPopupLevels, "Benchmark", new System.Data.DataTable());

            System.Data.DataTable comparisonTable = GetTableOutputPPT(leftPanel.ComparisonMenu, "Comparison", new System.Data.DataTable());

            System.Data.DataTable demogsTable = GetTableOutputPPT(leftPanel.FilterMenu, "FilterMenu", new System.Data.DataTable(), "Filter");

            System.Data.DataTable beveragesTable = GetTableOutputPPT(leftPanel.FilterMenu, "FilterMenu", new System.Data.DataTable(), "BeverageFilter");

            benchmarkTable.Merge(comparisonTable);
            var pptData = _reportGeneratorService.GetPPTBindingData("SPCountrycalculation", geographyTable, leftPanel.TimeperiodMenu.id, benchmarkTable, demogsTable, beveragesTable);
            var pptDataSlide9 = _reportGeneratorService.GetPPTBindingData("SPMultiSelectwithEquitywithSingleSelect_V3", geographyTable, leftPanel.TimeperiodMenu.id, benchmarkTable, demogsTable, beveragesTable);
            //var pptDataSlide9 = _reportGeneratorService.GetPPTBindingData("SPMultiSelectwithEquitywithSingleSelect_V3_occ_class", geographyTable, leftPanel.TimeperiodMenu.id, benchmarkTable, demogsTable, beveragesTable);
            var imagesList = _reportGeneratorService.GetImagesData(benchmarkTable);

            foreach (var image in imagesList)
            {
                if (image.HasImage)
                {
                    BlobClient blobClientImage = new BlobClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), Environment.GetEnvironmentVariable("ImagesContainer"), $"{image.ImageName}.png");
                    var streamImage = new MemoryStream();

                    blobClientImage.DownloadTo(streamImage);
                    //image.Image = Image.FromFile(@"D:\BEACH\API\API\Logos\"+image.ImageName+".png");
                    image.Image = Image.FromStream(streamImage);
                }
            }

            //SetLicense();
            List<string> firstSlideSectionsmetricType = new List<string>();
            List<string> secondSlideSectionsMetricType = new List<string>();
            if (leftPanel.GeographyMenu.GeographyId.Equals("India & Southwest Asia OU") || leftPanel.GeographyMenu.GeographyId.Equals("IND"))
            {
                secondSlideSectionsMetricType = SlideSections.secondSlideSectionsNonUS;
                FileName = "Beach_Template_India_21_02_2023_check.pptx";
            }
            else if (leftPanel.GeographyMenu.type.Equals("Total") || leftPanel.GeographyMenu.type.Equals("OU"))
            {
                secondSlideSectionsMetricType = SlideSections.secondSlideSectionsNonUS;
                FileName = "Beach_Template_NonUS_21_02_2023_check.pptx";
            }
            else if (leftPanel.GeographyMenu.GeographyId.Equals("USA"))
            {
                secondSlideSectionsMetricType = SlideSections.secondSlideSections;
                FileName = "Beach_Template_US_21_02_2023_check.pptx";
            }
            else
            {
                secondSlideSectionsMetricType = SlideSections.secondSlideSectionsNonUS;
                FileName = "Beach_Template_NonUS_21_02_2023_check.pptx";
            }
            MemoryStream ms = new MemoryStream();
            BlobClient blobClient = new BlobClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), Environment.GetEnvironmentVariable("InputContainerName"), $"{FileName}");
            var stream = new MemoryStream();

            blobClient.DownloadTo(stream);
            /*stream.Position = 0;*/

            // string fileName = @"D:\Kantar-Projects\Coca-Cola-Beach Project\DevOps-Repos\API\Temp\" + FileName;
            //using (Presentation pres = new Presentation(fileName))
            using (Presentation pres = new Presentation(stream))
            {
                DeleteSlides(leftPanel.ComparisonMenu.Count, pres);
                EditSelectionText(pres, leftPanel);
                GenerateOutputPPT(leftPanel.ComparisonMenu.Count, pres, pptData, 3, SlideSections.firstSlideSections, leftPanel, imagesList);
                GenerateOutputPPT(leftPanel.ComparisonMenu.Count, pres, pptData, 4, secondSlideSectionsMetricType, leftPanel, imagesList);
                GenerateOutputPPT(leftPanel.ComparisonMenu.Count, pres, pptDataSlide9, 7, leftPanel, imagesList); // 8, 9 & 14
                GenerateOutputPPT(leftPanel.ComparisonMenu.Count, pres, pptDataSlide9, 5, leftPanel, imagesList); // 6, 7 & 10
                GenerateOutputPPT(leftPanel.ComparisonMenu.Count, pres, pptDataSlide9, 11, leftPanel, imagesList); // 12 & 13
                //Not Required: GenerateOutputPPT(leftPanel.ComparisonMenu.Count, pres, pptDataSlide7, 9, leftPanel, imagesList);
                pres.Slides.Remove(pres.Slides[2]);
                pres.Slides.Remove(pres.Slides[12]);
                stream.Dispose();
                foreach (var image in imagesList)
                {
                    if (image.Image != null)
                    {
                        image.Image.Dispose();
                    }
                }
                pres.Save(ms, SaveFormat.Pptx);
            }
            return ms;
        }
        public MemoryStream FormatDataNew(LeftPanelRequest leftPanel, List<PPTBindingData> demog, List<PPTBindingData> Multi)
        {
            string FileName = string.Empty;
            List<PopupLevelData> geoPopupLevels = new List<PopupLevelData>();
            geoPopupLevels.Add(leftPanel.GeographyMenu);

            System.Data.DataTable geographyTable = GetTableOutputPPT(geoPopupLevels, "GeographyMenu", new System.Data.DataTable());

            List<PopupLevelData> benchamrkPopupLevels = new List<PopupLevelData>();
            benchamrkPopupLevels.Add(leftPanel.BenchmarkMenu);
            System.Data.DataTable benchmarkTable = GetTableOutputPPT(benchamrkPopupLevels, "Benchmark", new System.Data.DataTable());

            System.Data.DataTable comparisonTable = GetTableOutputPPT(leftPanel.ComparisonMenu, "Comparison", new System.Data.DataTable());

            System.Data.DataTable demogsTable = GetTableOutputPPT(leftPanel.FilterMenu, "FilterMenu", new System.Data.DataTable(), "Filter");

            System.Data.DataTable beveragesTable = GetTableOutputPPT(leftPanel.FilterMenu, "FilterMenu", new System.Data.DataTable(), "BeverageFilter");

            benchmarkTable.Merge(comparisonTable);
            var pptData = demog;
            var pptDataSlide9 = Multi;

            var imagesList = new List<ImagesList>();
            foreach (DataRow row in benchmarkTable.Rows)
            {
                imagesList.Add(new ImagesList()
                {
                    HasImage = false,
                    IsText = true,
                    ImageName = row.Field<string>("SelectionName"),
                    SelectionName = row.Field<string>("SelectionName")
                });
            }

            
            foreach (var image in imagesList)
            {
                if (image.HasImage)
                {
                    BlobClient blobClientImage = new BlobClient(_iConfig["Values:AzureWebJobsStorage"], _iConfig["Values:ImagesContainer"], $"{image.ImageName}.png");
                    var streamImage = new MemoryStream();

                    blobClientImage.DownloadTo(streamImage);
                    //image.Image = Image.FromFile(@"D:\BEACH\API\API\Logos\"+image.ImageName+".png");
                    image.Image = Image.FromStream(streamImage);
                }
            }

            SetLicense();
            List<string> firstSlideSectionsmetricType = new List<string>();
            List<string> secondSlideSectionsMetricType = new List<string>();
            if (leftPanel.GeographyMenu.GeographyId.Equals("India & Southwest Asia OU") || leftPanel.GeographyMenu.GeographyId.Equals("IND"))
            {
                secondSlideSectionsMetricType = SlideSections.secondSlideSectionsNonUS;
                FileName = "Beach_Template_India_21_02_2023_check.pptx";
            }
            else if (leftPanel.GeographyMenu.type.Equals("Total") || leftPanel.GeographyMenu.type.Equals("OU"))
            {
                secondSlideSectionsMetricType = SlideSections.secondSlideSectionsNonUS;
                FileName = "Beach_Template_NonUS_21_02_2023_check.pptx";
            }
            else if (leftPanel.GeographyMenu.GeographyId.Equals("USA"))
            {
                secondSlideSectionsMetricType = SlideSections.secondSlideSections;
                FileName = "Beach_Template_US_21_02_2023_check.pptx";
            }
            else
            {
                secondSlideSectionsMetricType = SlideSections.secondSlideSectionsNonUS;
                FileName = "Beach_Template_NonUS_21_02_2023_check.pptx";
            }
            MemoryStream ms = new MemoryStream();
            BlobClient blobClient = new BlobClient(_iConfig["Values:AzureWebJobsStorage"], _iConfig["Values:InputContainerName"], $"{FileName}");
            var stream = new MemoryStream();

            blobClient.DownloadTo(stream);
            /*stream.Position = 0;*/


            using (Presentation pres = new Presentation(stream))
            {
                DeleteSlides(leftPanel.ComparisonMenu.Count, pres);
                EditSelectionText(pres, leftPanel);
                GenerateOutputPPT(leftPanel.ComparisonMenu.Count, pres, pptData, 3, SlideSections.firstSlideSections, leftPanel, imagesList);
                GenerateOutputPPT(leftPanel.ComparisonMenu.Count, pres, pptData, 4, secondSlideSectionsMetricType, leftPanel, imagesList);
                GenerateOutputPPT(leftPanel.ComparisonMenu.Count, pres, pptDataSlide9, 7, leftPanel, imagesList); // 8, 9 & 14
                GenerateOutputPPT(leftPanel.ComparisonMenu.Count, pres, pptDataSlide9, 5, leftPanel, imagesList); // 6, 7 & 10
                GenerateOutputPPT(leftPanel.ComparisonMenu.Count, pres, pptDataSlide9, 11, leftPanel, imagesList); // 12 & 13
                //Not Required: GenerateOutputPPT(leftPanel.ComparisonMenu.Count, pres, pptDataSlide7, 9, leftPanel, imagesList);
                pres.Slides.Remove(pres.Slides[2]);


                pres.Slides.Remove(pres.Slides[12]);

                foreach (var image in imagesList)
                {
                    if (image.Image != null)
                    {
                        image.Image.Dispose();
                    }
                }
                pres.Save(ms, SaveFormat.Pptx);
            }
            return ms;
        }
        // Method for setting aspose license:
        private void SetLicense()
        {
            BlobClient blobClient = new BlobClient(_iConfig["Values:AzureWebJobsStorage"], _iConfig["Values:InputContainerName"], $"license.xml");
            var stream = new MemoryStream();
            Stream mystream = stream;
            blobClient.DownloadTo(stream);
            if (stream.Position > 0)
            {
                stream.Position = 0;
            }

            License license = new License();

            license.SetLicense(stream);
            stream.Dispose();
        }
        // Method for generating slide 4 & 5 output:
        private void GenerateOutputPPT(int numberOfCharts, Presentation pres, List<PPTBindingData> pptData, int SlideNumber, List<string> metricType, LeftPanelRequest leftPanel, List<ImagesList> imagesList)
        {
            // Bind slide 4 & 5:
            ISlide cur_Slide = pres.Slides[SlideNumber];
            BindSlideData(pptData, cur_Slide, numberOfCharts, metricType, SlideNumber, imagesList);
        }
        // Method for generating slide 8, 9 & 14 output:
        private void GenerateOutputPPT(int numberOfCharts, Presentation pres, List<PPTBindingData> pptData, int SlideNumber, LeftPanelRequest leftPanel, List<ImagesList> imagesList)
        {
            if (SlideNumber == 7) // for slide 8, 9 & 14:
            {
                ISlide cur_Slide8 = pres.Slides[SlideNumber]; // slide 8 
                ISlide cur_Slide9 = pres.Slides[SlideNumber + 1]; // slide 9 
                ISlide cur_Slide11 = pres.Slides[10]; // slide 15 
                ISlide cur_Slide15 = pres.Slides[14]; // slide 15 

                BindSlideData(pptData, cur_Slide8, numberOfCharts, SlideNumber, imagesList); // slide 8
                BindSlideNineData(pptData, cur_Slide9, numberOfCharts, SlideNumber + 1, imagesList); // slide 9
                BindSlideElevenData(pptData, cur_Slide11, numberOfCharts, 10, imagesList); // slide 11
                BindSlideFifteenData(pptData, cur_Slide15, numberOfCharts, 14, imagesList); // slide 15
            }
            else if (SlideNumber == 5) // for slide 6, 7 & 10:
            {
                ISlide cur_Slide6 = pres.Slides[SlideNumber]; // slide 6
                ISlide cur_Slide7 = pres.Slides[SlideNumber + 1]; // slide 7 
                ISlide cur_Slide10 = pres.Slides[9]; // slide 10

                BindSlideSixSevenAndTenData(pptData, cur_Slide6, numberOfCharts, 5, imagesList); // slide 6
                BindSlideSixSevenAndTenData(pptData, cur_Slide7, numberOfCharts, 6, imagesList); // slide 7
                BindSlideSixSevenAndTenData(pptData, cur_Slide10, numberOfCharts, 9, imagesList); // slide 10
            }
            else
            {
                ISlide cur_Slide12 = pres.Slides[SlideNumber]; // slide 12
                ISlide cur_Slide13 = pres.Slides[SlideNumber + 1]; // slide 13

                BindSlideTwelveData(pptData, cur_Slide12, numberOfCharts, 11, imagesList); // slide 12
                BindSlideThirteenData(pptData, cur_Slide13, numberOfCharts, 12, imagesList); // slide 13
            }
        }
        private void EditSelectionText(Presentation pres, LeftPanelRequest leftPanel)
        {
            ISlide firstSlide = pres.Slides[0];
            IGroupShape tempGroup;
            //IAutoShape tempShape;
            tempGroup = (IGroupShape)firstSlide.Shapes.Where(x => x.Name == "SelectionSummary").FirstOrDefault();

            // Change Time Period Selection Text
            //tempShape = (IAutoShape)tempGroup.Shapes.Where(x => x.Name == "TimePeriod").FirstOrDefault();
            var tempShape1 = (IAutoShape)firstSlide.Shapes.Where(x => x.Name == "TimePeriodText").FirstOrDefault();
            string timePeriodSelection = $"Time Period: {leftPanel.TimeperiodMenu.text}";
            tempShape1.TextFrame.Text = $"{leftPanel.TimeperiodMenu.text}";

            // Change Benchmark Selection Text
            //tempShape = (IAutoShape)tempGroup.Shapes.Where(x => x.Name == "Benchmark").FirstOrDefault();
            var tempShape2 = (IAutoShape)firstSlide.Shapes.Where(x => x.Name == "BenchmarkText").FirstOrDefault();
            string benchmarkText = new List<string> { "BRAND", "TRADEMARK" }.Contains(leftPanel.BenchmarkMenu.type.ToUpper()) ? (leftPanel.BenchmarkMenu.detailedText + ": " + leftPanel.BenchmarkMenu.consumptionName) : leftPanel.BenchmarkMenu.text;
            string benchmarkSekectionText = $"Benchmark: {leftPanel.BenchmarkMenu.type}: {benchmarkText}";
            tempShape2.TextFrame.Text = $"{leftPanel.BenchmarkMenu.type}: {benchmarkText}";

            // Change Comparison Selection Text
            var compareTextArray = (from r in leftPanel.ComparisonMenu.AsEnumerable()
                                    select new List<string> { "BRAND", "TRADEMARK" }.Contains(r.type.ToUpper()) ? (r.detailedText + ": " + r.consumptionName) : r.text
                              ).ToArray();
            var compareSelectionTextJoin = string.Join(", ", compareTextArray);
            //tempShape = (IAutoShape)tempGroup.Shapes.Where(x => x.Name == "Comparison").FirstOrDefault();
            var tempShape3 = (IAutoShape)firstSlide.Shapes.Where(x => x.Name == "ComparisonText").FirstOrDefault();
            string compareSelectionText = $"Comparison: {compareSelectionTextJoin}";
            tempShape3.TextFrame.Text = $"{compareSelectionTextJoin}";

            // Change Geography Selection Text
            //tempShape = (IAutoShape)tempGroup.Shapes.Where(x => x.Name == "Geography").FirstOrDefault();
            var tempShape4 = (IAutoShape)firstSlide.Shapes.Where(x => x.Name == "GeographyText").FirstOrDefault();
            string geoSelectionText = $"Geography: {leftPanel.GeographyMenu.detailedText}";
            tempShape4.TextFrame.Text = $"{leftPanel.GeographyMenu.detailedText}";

            // Change Filter Selection Text
            var filterTextArray = (from r in leftPanel.FilterMenu.AsEnumerable()
                                   select new List<string> { "BEVERAGEFILTER" }.Contains(r.type.ToUpper()) && r.levelId >= 4 ? (r.detailedText + ": " + r.consumptionName)
                                   : (new List<string> { "FILTER" }.Contains(r.type.ToUpper()) && r.detailedText != null ? r.detailedText : r.text)
                              ).ToArray();
            var filterSelectionTextJoin = string.Join(", ", filterTextArray);
            //tempShape = (IAutoShape)tempGroup.Shapes.Where(x => x.Name == "Filter").FirstOrDefault();
            var tempShape5 = (IAutoShape)firstSlide.Shapes.Where(x => x.Name == "FilterText").FirstOrDefault();
            string filterSelectionText = $"Filters: {filterSelectionTextJoin}";
            tempShape5.TextFrame.Text = $"{filterSelectionTextJoin}";

            // For rest of the slides 
            //Exmaple: Selections: Time Period: 2022 || Benchmark: Detailed Categories – SSD Regular || Comparison: Coca-Cola, Dr Pepper, Pepsi, 7-Up, Fanta, Sprite || Geography: India || Filter: Demographics

            var slidesCount = pres.Slides.Count;
            ISlide restSlides;
            IAutoShape selectionTextShape;
            string selectionText = $"Selection: {timePeriodSelection} || {benchmarkSekectionText} || {compareSelectionText} || {geoSelectionText} || {filterSelectionText}";
            for (int i = 2; i < 15; i++)
            {
                restSlides = pres.Slides[i];
                selectionTextShape = (IAutoShape)restSlides.Shapes.Where(x => x.Name == "SelectionText").FirstOrDefault();
                selectionTextShape.TextFrame.Text = selectionText;
                selectionTextShape.TextFrame.Paragraphs[0].Portions[0].PortionFormat.FontBold = NullableBool.False;
            }
        }
        // Method for binding slide 4 & 5 data:
        private void BindSlideData(List<PPTBindingData> pptData, ISlide cur_Slide, int numberOfCharts, List<string> metricType, int SlideNumber, List<ImagesList> imagesLists)
        {
            // Code for binding slide with bar charts
            int index = 0;
            int row = 2;
            int column = 1;
            var slideFourData = pptData.Where(x => x.SlideNumber == SlideNumber + 1).ToList();
            numberOfCharts++;
            slideFourData = slideFourData.OrderBy(a => a.GroupSort).ThenBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ThenBy(a => a.SortId).ToList();
            var selectionList = slideFourData.Select(a => new { a.SelectionType, a.SelectionId, a.SelectionName, a.ConsumptionID, a.ConsumptionName }).Distinct().OrderBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ToList();

            #region Replace Brand Image
            ITable tbl = (ITable)cur_Slide.Shapes.FirstOrDefault(x => x.Name == "Table 11");
            for (int i = 1; i <= numberOfCharts; i++)
            {
                if (selectionList[i - 1].ConsumptionID != 1)
                {
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName + ": " + selectionList[i - 1].ConsumptionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.Black;
                    format.FontBold = NullableBool.False;
                    format.FontHeight = (float)11;
                    format.LatinFont = new FontData("TCCC-UnityText");
                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }
                else if ((imagesLists.FindIndex(e => e.SelectionName == selectionList[i - 1].SelectionName) != -1) && (imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).HasImage))
                {
                    ((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i)).Hidden = false;
                    imageReplace((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i), imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).Image);
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.White;
                }
                else
                {
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.Black;
                    format.FontBold = NullableBool.False;
                    format.FontHeight = (float)11;
                    format.LatinFont = new FontData("TCCC-UnityText");
                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }

            }
            #endregion

            #region Data Binding
            foreach (var item in metricType)
            {
                int count = 1;
                while (count <= numberOfCharts)
                {
                    string chartName = $"{item}_chart_{count}";
                    IChart barChart = (IChart)cur_Slide.Shapes.FirstOrDefault(x => x.Name.ToLower() == chartName.ToLower());
                    IChartDataWorkbook barWorkbook = barChart.ChartData.ChartDataWorkbook;
                    string selectionName = slideFourData[index].SelectionName;
                    int selectionId = slideFourData[index].SelectionId;
                    var genderData = slideFourData.Where(x => x.MetricType.Contains(item) && x.SelectionId == selectionId && x.SelectionName == selectionName).ToList();
                    double metricPercentage = 0.0;
                    int seriesCount = genderData.Select(x => new { x.Metric }).Distinct().Count();
                    int categoryCount = genderData.Select(x => new { x.Metric }).Distinct().Count();//2
                    var categoryName = genderData.Select(x => new { x.Metric }).Distinct().ToArray();//2
                    barChart.TextFormat.ParagraphFormat.Alignment = TextAlignment.Right;

                    int wbRowIndex = 0;
                    int wbColumnIndex = 0;
                    bool isEthnicity = false;
                    if (item.ToLower() == "ethnicity")
                    {
                        isEthnicity = true;
                    }
                    double secPercentage = 0;
                    for (int i = 0; i < seriesCount; i++)
                    {
                        bool isMultiCultural = false;
                        if (genderData[i].Metric.ToLower() == "multicultural")
                        {
                            isMultiCultural = true;
                        }
                        if (!isEthnicity)
                        {
                            barWorkbook.GetCell(0, i + 1, 0, genderData[i].Metric);
                        }
                        int r = i;
                        if (SlideSections.isReversed.ContainsKey(item) && SlideSections.isReversed[item])
                        {
                            r = seriesCount - i - 1;
                        }
                        tbl[(column + count - 1), (row + r)].TextFrame.Text = (Math.Round(Convert.ToDouble(genderData[i].Percentage * 100), 1).ToString("#0.0") + "%");
                        tbl[(column + count - 1), (row + r)].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.FillType = FillType.Solid;
                        tbl[(column + count - 1), (row + r)].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)genderData[i].Significance);
                        wbColumnIndex = 0;
                        if (item.Equals("SEC", StringComparison.OrdinalIgnoreCase))
                        {
                            secPercentage += (double)genderData[i].Percentage;
                        }

                        for (int j = 0; (j < seriesCount) && !isMultiCultural; j++)
                        {
                            metricPercentage = 0.0;
                            metricPercentage = (double)genderData[i].Percentage;
                            double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);

                            if (!isEthnicity)
                            {
                                if (item.Equals("Kids", StringComparison.OrdinalIgnoreCase))
                                {
                                    barWorkbook.GetCell(0, wbColumnIndex + 1, wbRowIndex + 1, tempMetricPercentage);
                                }
                                else if (item.Equals("SEC", StringComparison.OrdinalIgnoreCase))
                                {
                                    barWorkbook.GetCell(0, wbColumnIndex + 1, seriesCount - wbRowIndex, tempMetricPercentage);
                                    if (i == 2)
                                    {
                                        double tempSecPercentage = Convert.ToDouble(metricPercentage > 1 ? (100 - secPercentage) / 100 : (1 - secPercentage));
                                        barWorkbook.GetCell(0, wbColumnIndex + 1, wbRowIndex + 2, tempSecPercentage);
                                    }
                                }
                                else
                                {
                                    barWorkbook.GetCell(0, wbRowIndex + 1, wbColumnIndex + 1, tempMetricPercentage);
                                }
                            }
                            else
                            {
                                barWorkbook.GetCell(0, wbColumnIndex + 1, wbRowIndex + 1, tempMetricPercentage);
                            }
                            wbColumnIndex++;
                        }
                        if (!isMultiCultural)
                        {
                            wbRowIndex++;
                        }
                    }
                    count++;
                    index += categoryCount;
                    if ((count - 1) == numberOfCharts)
                    {
                        row = row + categoryCount + 1;
                    }
                }
            }
            #endregion
        }
        // Method for binding slide 8 data:
        private void BindSlideData(List<PPTBindingData> pptData, ISlide cur_Slide, int numberOfCharts, int SlideNumber, List<ImagesList> imagesLists)
        {
            int index = 0;
            var slideEightData = pptData.Where(x => x.SlideNumber == SlideNumber + 1).ToList();
            slideEightData = slideEightData.OrderBy(a => a.GroupSort).ThenBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ThenBy(a => a.SortId).ToList();
            var selectionList = slideEightData.Select(a => new { a.SelectionType, a.SelectionId, a.SelectionName, a.ConsumptionID, a.ConsumptionName }).Distinct().OrderBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ToList();

            #region 4brands_when_ww
            double metricPercentage = 0.0;
            string chartName = "4brands_when_ww";
            IChart barChart = (IChart)cur_Slide.Shapes.FirstOrDefault(x => x.Name.ToLower() == chartName.ToLower());
            IChartDataWorkbook barWorkbook = barChart.ChartData.ChartDataWorkbook;
            string selectionName = slideEightData[index].SelectionName;
            int selectionId = slideEightData[index].SelectionId;
            var benchamrkData = slideEightData.Where(x => x.MetricType.Contains("Weekday/Weekend") && x.SelectionType == "Benchmark").ToList();
            int seriesCount = benchamrkData.Select(x => new { x.Metric }).Distinct().Count();
            for (int i = 0; i < barChart.ChartData.Series.Count; i++)
            {
                for (int j = 0; j < barChart.ChartData.Series[i].Labels.Count; j++)
                {
                    barChart.ChartData.Series[i].Labels[j].DataLabelFormat.NumberFormat = "0.0%";
                    barChart.ChartData.Series[i].Labels[j].DataLabelFormat.IsNumberFormatLinkedToSource = false;
                }
            }
            ITable whenTable = (ITable)cur_Slide.Shapes.FirstOrDefault(x => x.Name.ToLower() == "whentable");
            // Bind benchmark data 4brands_when_ww:
            for (int i = 7; i >= 1; i--)
            {
                barWorkbook.GetCell(0, 1, 0, benchamrkData[index].SelectionName);
                var obj = selectionList.Find(a => a.SelectionName == benchamrkData[index].SelectionName && a.SelectionId == benchamrkData[index].SelectionId);
                whenTable[0, 0].TextFrame.Text = obj.ConsumptionID == 1 ? benchamrkData[index].SelectionName : obj.SelectionName + ": " + obj.ConsumptionName;
                metricPercentage = 0.0;
                metricPercentage = (double)benchamrkData[index].Percentage;
                double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);
                barWorkbook.GetCell(0, 1, i, tempMetricPercentage);
                IDataLabel lbl = barChart.ChartData.Series[i - 1].DataPoints[0].Label;
                lbl.DataLabelFormat.Format.Fill.FillType = FillType.Solid;
                lbl.DataLabelFormat.Format.Fill.SolidFillColor.Color = Color.FromArgb(120, 242, 242, 242);
                lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)benchamrkData[index].Significance);
                index++;
            }

            index = 0;

            // Bind comparison data 4brands_when_ww:
            var comparisonData = slideEightData.Where(x => x.MetricType.Contains("Weekday/Weekend") && x.SelectionType == "Comparison").ToList();
            for (int i = 2; i <= selectionList.Count; i++)
            {
                var item = selectionList[i - 1];

                if (item.SelectionType == "Comparison")
                {
                    var dataList = comparisonData.Where(x => x.SelectionName == item.SelectionName && x.SelectionId == item.SelectionId).ToList();

                    for (int j = 7; j >= 0; j--)
                    {
                        if (j == 0)
                        {
                            barWorkbook.GetCell(0, i, j, item.SelectionName);
                            var obj = selectionList.Find(a => a.SelectionName == item.SelectionName && a.SelectionId == item.SelectionId);
                            whenTable[i - 1, 0].TextFrame.Text = obj.ConsumptionID == 1 ? item.SelectionName : obj.SelectionName + ": " + obj.ConsumptionName;
                        }
                        else
                        {
                            metricPercentage = 0.0;
                            metricPercentage = (double)dataList[index].Percentage;
                            double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);
                            barWorkbook.GetCell(0, i, j, tempMetricPercentage);
                            IDataLabel lbl = barChart.ChartData.Series[j - 1].DataPoints[i - 1].Label;
                            lbl.DataLabelFormat.Format.Fill.FillType = FillType.Solid;
                            lbl.DataLabelFormat.Format.Fill.SolidFillColor.Color = Color.FromArgb(120, 242, 242, 242);
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)dataList[index].Significance);
                        }
                        index++;
                    }
                    index = 0;
                }
            }
            #endregion

            #region 4brands_when_daypart
            double metricPercentageDaypart = 0.0;
            string chartNameDaypart = "4brands_when_daypart";
            IChart barChartDaypart = (IChart)cur_Slide.Shapes.FirstOrDefault(x => x.Name.ToLower() == chartNameDaypart.ToLower());
            IChartDataWorkbook barWorkbookDaypart = barChartDaypart.ChartData.ChartDataWorkbook;
            string selectionNameDaypart = slideEightData[index].SelectionName;
            int selectionIdDaypart = slideEightData[index].SelectionId;
            var benchamrkDataDaypart = slideEightData.Where(x => x.MetricType.Contains("Daypart") && x.SelectionType == "Benchmark").ToList();
            int seriesCountDaypart = benchamrkDataDaypart.Select(x => new { x.Metric }).Distinct().Count();

            for (int i = 0; i < barChartDaypart.ChartData.Series.Count; i++)
            {
                for (int j = 0; j < barChartDaypart.ChartData.Series[i].Labels.Count; j++)
                {
                    barChartDaypart.ChartData.Series[i].Labels[j].DataLabelFormat.NumberFormat = "0.0%";
                    barChartDaypart.ChartData.Series[i].Labels[j].DataLabelFormat.IsNumberFormatLinkedToSource = false;
                }
            }

            // Bind benchmark data 4brands_when_daypart:
            for (int i = 0; i <= 7; i++)
            {
                if (i == 0)
                {
                    var obj = selectionList.Find(a => a.SelectionName == benchamrkDataDaypart[index].SelectionName && a.SelectionId == benchamrkDataDaypart[index].SelectionId);
                    barWorkbookDaypart.GetCell(0, 1, i, obj.ConsumptionID == 1 ? obj.SelectionName : obj.SelectionName + ": " + obj.ConsumptionName);
                }
                else
                {
                    metricPercentageDaypart = 0.0;
                    metricPercentageDaypart = (double)benchamrkDataDaypart[index].Percentage;
                    double tempMetricPercentage = Convert.ToDouble(metricPercentageDaypart > 1 ? metricPercentageDaypart / 100 : metricPercentageDaypart);
                    barWorkbookDaypart.GetCell(0, 1, i, tempMetricPercentage);
                    IDataLabel lbl = barChartDaypart.ChartData.Series[i - 1].DataPoints[0].Label;
                    lbl.DataLabelFormat.Format.Fill.FillType = FillType.Solid;
                    lbl.DataLabelFormat.Format.Fill.SolidFillColor.Color = Color.FromArgb(120, 242, 242, 242);
                    lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                    lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)benchamrkDataDaypart[index].Significance);
                    index++;
                }
            }

            index = 0;
            // Bind comparison data 4brands_when_daypart:
            var comparisonDataDaypart = slideEightData.Where(x => x.MetricType.Contains("Daypart") && x.SelectionType == "Comparison").ToList();
            for (int i = 2; i <= selectionList.Count; i++)
            {
                var item = selectionList[i - 1];

                if (item.SelectionType == "Comparison")
                {
                    var dataList = comparisonDataDaypart.Where(x => x.SelectionName == item.SelectionName && x.SelectionId == item.SelectionId).ToList();

                    for (int j = 0; j <= 7; j++)
                    {
                        if (j != 0)
                        {
                            metricPercentageDaypart = 0.0;
                            metricPercentageDaypart = (double)dataList[index].Percentage;
                            double tempMetricPercentage = Convert.ToDouble(metricPercentageDaypart > 1 ? metricPercentageDaypart / 100 : metricPercentageDaypart);
                            barWorkbookDaypart.GetCell(0, i, j, tempMetricPercentage);
                            IDataLabel lbl = barChartDaypart.ChartData.Series[j - 1].DataPoints[i - 1].Label;
                            lbl.DataLabelFormat.Format.Fill.FillType = FillType.Solid;
                            lbl.DataLabelFormat.Format.Fill.SolidFillColor.Color = Color.FromArgb(120, 242, 242, 242);
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)dataList[index].Significance);
                            index++;
                        }
                        else
                        {
                            var obj = selectionList.Find(a => a.SelectionName == item.SelectionName && a.SelectionId == item.SelectionId);
                            barWorkbookDaypart.GetCell(0, i, j, obj.ConsumptionID == 1 ? obj.SelectionName : obj.SelectionName + ": " + obj.ConsumptionName);
                        }

                    }
                    index = 0;
                }
            }
            #endregion
        }
        // Method for binding slide 6, 7 & 10:
        private void BindSlideSixSevenAndTenData(List<PPTBindingData> pptData, ISlide cur_Slide, int numberOfCharts, int SlideNumber, List<ImagesList> imagesLists)
        {
            int index = 0;
            numberOfCharts++;
            var slideData = pptData.Where(x => x.SlideNumber == SlideNumber + 1).ToList();
            //slideData = slideData.OrderBy(a => a.GroupSort).ThenBy(a => a.SelectionType).ThenBy(a => a.SortId).ToList();
            var selectionList = slideData.Select(a => new { a.SelectionType, a.SelectionId, a.SelectionName, a.ConsumptionID, a.ConsumptionName }).Distinct().OrderBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ToList();
            double maxValue = (double)slideData.Max(e => e.Percentage);
            maxValue = maxValue < 0.9 ? maxValue + 0.1 : 1;

            #region Slide 6, 7 and 10 Image Replacement
            IAutoShape selectionTextShape;
            for (int i = 1; i <= numberOfCharts; i++)
            {
                if (selectionList[i - 1].ConsumptionID != 1)
                {
                    selectionTextShape = (IAutoShape)cur_Slide.Shapes.Where(x => x.Name == "BrandText" + i).FirstOrDefault();
                    selectionTextShape.Hidden = false;
                    selectionTextShape.TextFrame.Text = selectionList[i - 1].SelectionName + ": " + selectionList[i - 1].ConsumptionName;

                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }
                else if ((imagesLists.FindIndex(e => e.SelectionName == selectionList[i - 1].SelectionName) != -1) && (imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).HasImage))
                {
                    ((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i)).Hidden = false;
                    imageReplace((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i), imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).Image);
                    selectionTextShape = (IAutoShape)cur_Slide.Shapes.Where(x => x.Name == "BrandText" + i).FirstOrDefault();
                    selectionTextShape.Hidden = true;
                }
                else
                {
                    selectionTextShape = (IAutoShape)cur_Slide.Shapes.Where(x => x.Name == "BrandText" + i).FirstOrDefault();
                    selectionTextShape.Hidden = false;
                    selectionTextShape.TextFrame.Text = selectionList[i - 1].SelectionName;

                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }
            }
            #endregion

            #region Slide 6 Data Binding
            if (cur_Slide.SlideNumber == 6)
            {
                for (int i = 1; i <= numberOfCharts; i++)
                {
                    string chartName = $"Chart{i}";
                    IChartSeries Series;
                    IChart barChart = (IChart)cur_Slide.Shapes.FirstOrDefault(x => x.Name.ToLower() == chartName.ToLower());
                    IChartDataWorkbook barWorkbook = barChart.ChartData.ChartDataWorkbook;
                    barChart.Axes.VerticalAxis.MaxValue = maxValue;
                    string selectionName = slideData[index].SelectionName;
                    double metricPercentage = 0.0;
                    int c = barChart.ChartData.Series.Count;
                    Series = barChart.ChartData.Series[0];
                    for (int j = 0; j < 9; j++)
                    {
                        metricPercentage = 0.0;
                        metricPercentage = (double)slideData[index].Percentage;
                        double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);
                        string strColor = GetColor(slideData[index].Metric);
                        Series.DataPoints[j].Format.Fill.SolidFillColor.Color = ColorTranslator.FromHtml(strColor);
                        barWorkbook.GetCell(0, j + 1, 0, slideData[index].Metric);
                        barWorkbook.GetCell(0, j + 1, 1, tempMetricPercentage);
                        IDataLabel lbl = barChart.ChartData.Series[0].DataPoints[j].Label;
                        lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                        lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)slideData[index].Significance);
                        index++;
                    }
                }
            }
            #endregion

            #region Slide 7 & 10  Data Binding
            else
            {
                for (int i = 1; i <= numberOfCharts; i++)
                {
                    string chartName = $"Chart{i}";

                    IChart barChart = (IChart)cur_Slide.Shapes.FirstOrDefault(x => x.Name.ToLower() == chartName.ToLower());
                    IChartDataWorkbook barWorkbook = barChart.ChartData.ChartDataWorkbook;
                    barChart.Axes.HorizontalAxis.MaxValue = maxValue;
                    string selectionName = slideData[index].SelectionName;
                    double metricPercentage = 0.0;
                    int c = barChart.ChartData.Series.Count;
                    for (int j = 0; j < 10; j++)
                    {
                        metricPercentage = 0.0;
                        metricPercentage = (double)slideData[index].Percentage;
                        double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);
                        barWorkbook.GetCell(0, 0, j + 1, slideData[index].Metric);
                        barWorkbook.GetCell(0, 1, j + 1, tempMetricPercentage);
                        IDataLabel lbl = barChart.ChartData.Series[0].DataPoints[j].Label;
                        lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                        lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)slideData[index].Significance);
                        index++;
                    }
                }
            }
            #endregion
        }
        // Method for binding slide 9 data:
        private void BindSlideNineData(List<PPTBindingData> pptData, ISlide cur_Slide, int numberOfCharts, int SlideNumber, List<ImagesList> imagesLists)
        {
            numberOfCharts++;
            var slideNineData = pptData.Where(x => x.SlideNumber == SlideNumber + 1).ToList();
            var whereHL = slideNineData.Where(x => x.MetricType == "Where_Group_HL").ToList();
            var whereLL = slideNineData.Where(x => x.MetricType == "Where_Group_LL").ToList();
            slideNineData = slideNineData.OrderBy(a => a.GroupSort).ThenBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ThenBy(a => a.SortId).ToList();
            var selectionList = slideNineData.Select(a => new { a.SelectionType, a.SelectionId, a.SelectionName, a.ConsumptionID, a.ConsumptionName }).Distinct().OrderBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ToList();
            int index = 0;
            int chartCount = 0;
            double maxValue = (double)whereLL.Max(e => e.Percentage);
            maxValue = maxValue < 0.9 ? maxValue + 0.1 : 1;
            #region Replace Images
            IAutoShape selectionTextShape;
            for (int i = 1; i <= numberOfCharts; i++)
            {
                if (selectionList[i - 1].ConsumptionID != 1)
                {
                    selectionTextShape = (IAutoShape)cur_Slide.Shapes.Where(x => x.Name == "BrandText" + i).FirstOrDefault();
                    selectionTextShape.Hidden = false;
                    selectionTextShape.TextFrame.Text = selectionList[i - 1].SelectionName + ": " + selectionList[i - 1].ConsumptionName;

                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }
                else if ((imagesLists.FindIndex(e => e.SelectionName == selectionList[i - 1].SelectionName) != -1) && (imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).HasImage))
                {
                    ((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i)).Hidden = false;
                    imageReplace((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i), imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).Image);
                    selectionTextShape = (IAutoShape)cur_Slide.Shapes.Where(x => x.Name == "BrandText" + i).FirstOrDefault();
                    selectionTextShape.Hidden = true;
                }
                else
                {
                    selectionTextShape = (IAutoShape)cur_Slide.Shapes.Where(x => x.Name == "BrandText" + i).FirstOrDefault();
                    selectionTextShape.Hidden = false;
                    selectionTextShape.TextFrame.Text = selectionList[i - 1].SelectionName;

                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }

            }
            #endregion

            #region Slide 9 data binding
            foreach (var item in selectionList)
            {
                string barChartName = $"ColChart{chartCount + 1}";
                string donChartName = $"DonChart{chartCount + 1}";
                IChart barChart = (IChart)cur_Slide.Shapes.FirstOrDefault(x => x.Name.ToLower() == barChartName.ToLower());
                IChartDataWorkbook barWorkbook = barChart.ChartData.ChartDataWorkbook;
                barChart.Axes.HorizontalAxis.MaxValue = maxValue;

                IChart donChart = (IChart)cur_Slide.Shapes.FirstOrDefault(x => x.Name.ToLower() == donChartName.ToLower());
                IChartDataWorkbook donWorkbook = donChart.ChartData.ChartDataWorkbook;

                string selectionName = whereLL[index].SelectionName;

                double metricPercentage = 0.0;
                if (item.SelectionType == "Benchmark")
                {
                    // Benchmark data binding for column chart slide 9:
                    var benchamrkDataLL = whereLL.Where(x => x.SelectionType == "Benchmark").ToList();
                    var benchamrkDataHL = whereHL.Where(x => x.SelectionType == "Benchmark").ToList();
                    for (int j = 1; j < 2; j++)
                    {
                        for (int k = 0; k <= 1; k++)
                        {
                            metricPercentage = 0.0;
                            metricPercentage = (double)benchamrkDataHL[k].Percentage;
                            double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);
                            donWorkbook.GetCell(0, k + 1, j, tempMetricPercentage);
                            IDataLabel lbl = donChart.ChartData.Series[0].DataPoints[k].Label;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)benchamrkDataHL[k].Significance);
                        }
                        for (int k = 0; k <= 7; k++)
                        {
                            metricPercentage = 0.0;
                            metricPercentage = (double)benchamrkDataLL[k].Percentage;
                            double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);
                            barWorkbook.GetCell(0, k + 1, j, tempMetricPercentage);
                            IDataLabel lbl = barChart.ChartData.Series[0].DataPoints[k].Label;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)benchamrkDataLL[k].Significance);
                        }
                    }
                }
                else
                {
                    // Comparison data binding for column chart slide 9:
                    var compareDataLL = whereLL.Where(x => x.SelectionType == "Comparison" && x.SelectionName == item.SelectionName && x.SelectionId == item.SelectionId).ToList();
                    var compareDataHL = whereHL.Where(x => x.SelectionType == "Comparison" && x.SelectionName == item.SelectionName && x.SelectionId == item.SelectionId).ToList();
                    for (int j = 1; j < 2; j++)
                    {
                        for (int k = 0; k <= 1; k++)
                        {
                            metricPercentage = 0.0;
                            metricPercentage = (double)compareDataHL[k].Percentage;
                            double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);
                            donWorkbook.GetCell(0, k + 1, j, tempMetricPercentage);
                            IDataLabel lbl = donChart.ChartData.Series[0].DataPoints[k].Label;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)compareDataHL[k].Significance);
                        }
                        for (int k = 0; k <= 7; k++)
                        {
                            metricPercentage = 0.0;
                            metricPercentage = (double)compareDataLL[k].Percentage;
                            double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);
                            barWorkbook.GetCell(0, k + 1, j, tempMetricPercentage);
                            IDataLabel lbl = barChart.ChartData.Series[0].DataPoints[k].Label;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)compareDataLL[k].Significance);
                        }
                    }
                }
                chartCount++;
            }
            #endregion
        }
        // Method for binding slide 11 data:
        private void BindSlideTwelveData(List<PPTBindingData> pptData, ISlide cur_Slide, int numberOfCharts, int SlideNumber, List<ImagesList> imagesLists)
        {
            numberOfCharts++;
            var slideData = pptData.Where(x => x.SlideNumber == SlideNumber + 1).ToList();
            slideData = slideData.OrderBy(a => a.GroupSort).ThenBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ThenBy(a => a.SortId).ToList();
            var selectionList = slideData.Select(a => new { a.SelectionType, a.SelectionId, a.SelectionName, a.ConsumptionID, a.ConsumptionName }).Distinct().OrderBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ToList();
            int index = 0;
            int chartCount = 0;

            #region Slide 11 Image Replacement
            ITable tbl = (ITable)cur_Slide.Shapes.FirstOrDefault(x => x.Name == "Table 11");
            for (int i = 1; i <= numberOfCharts; i++)
            {
                if (selectionList[i - 1].ConsumptionID != 1)
                {
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName + ": " + selectionList[i - 1].ConsumptionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.Black;
                    format.FontBold = NullableBool.False;
                    format.FontHeight = (float)11;
                    format.LatinFont = new FontData("TCCC-UnityText");
                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }
                else if ((imagesLists.FindIndex(e => e.SelectionName == selectionList[i - 1].SelectionName) != -1) && (imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).HasImage))
                {
                    ((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i)).Hidden = false;
                    imageReplace((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i), imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).Image);
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.White;
                }
                else
                {
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.Black;
                    format.FontBold = NullableBool.False;
                    format.FontHeight = (float)11;
                    format.LatinFont = new FontData("TCCC-UnityText");
                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }
            }
            #endregion

            for (int i = 1; i <= numberOfCharts; i++)
            {
                for (int j = 1; j <= 10; j++)
                {
                    if (!(new List<int> { 7 }.Contains(j)))
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("textBox" + i + "" + j)).Hidden = true;
                }
            }

            #region Slide 11 Data Binding
            foreach (var item in selectionList)
            {
                string chartName = $"Chart{chartCount + 1}";

                IChart barChart = (IChart)cur_Slide.Shapes.FirstOrDefault(x => x.Name.ToLower() == chartName.ToLower());
                IChartDataWorkbook barWorkbook = barChart.ChartData.ChartDataWorkbook;
                //string selectionName = slideData[index].SelectionName;
                barChart.TextFormat.ParagraphFormat.Alignment = TextAlignment.Right;
                var benchamrkData = slideData.Where(x => x.SelectionType == "Benchmark").ToList();
                double metricPercentage = 0.0;

                if (item.SelectionType == "Benchmark")
                {
                    // Benchmark data binding for slide 11:
                    for (int i = 1; i <= 10; i++)
                    {
                        if (i != 7)
                        {
                            metricPercentage = 0.0;
                            metricPercentage = (double)benchamrkData[index].Percentage;
                            bool isCategory = benchamrkData[index].IsCategory == 1 ? true : false;
                            for (int j = 1; j <= 2; j++)
                            {
                                if (j == 2)
                                {
                                    metricPercentage = 1 - metricPercentage;
                                }
                                double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);

                                if (!isCategory)
                                {
                                    barWorkbook.GetCell(0, i, j, tempMetricPercentage);
                                    tbl[chartCount + 1, i].TextFrame.Text = (Math.Round(Convert.ToDouble(benchamrkData[index].Percentage * 100), 1).ToString("#0.0") + "%");
                                    tbl[chartCount + 1, i].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.FillType = FillType.Solid;
                                    tbl[chartCount + 1, i].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)benchamrkData[index].Significance);
                                }
                                else
                                {
                                    barChart.Hidden = true;
                                    //tbl[chartCount + 1, i].TextFrame.Text = "NA";
                                    //tbl[chartCount + 1, i].TextFrame.Paragraphs[0].ParagraphFormat.Alignment = TextAlignment.Center;
                                    tbl[chartCount + 1, i].TextFrame.Text = "";
                                    cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("textBox" + (chartCount + 1) + "" + i)).Hidden = false;
                                }
                                /* IDataLabel lbl = barChart.ChartData.Series[j - 1].DataPoints[i - 1].Label;
                                 lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                                 lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)benchamrkData[index].Significance);*/

                            }
                            index++;
                        }
                    }
                }

                else
                {
                    index = 0;
                    var compareData = slideData.Where(x => x.SelectionType == "Comparison" && x.SelectionName == item.SelectionName && x.SelectionId == item.SelectionId).ToList();
                    for (int i = 1; i <= 10; i++)
                    {
                        if (i != 7)
                        {
                            bool isCategory = compareData[index].IsCategory == 1 ? true : false;
                            metricPercentage = 0.0;
                            metricPercentage = (double)compareData[index].Percentage;
                            for (int j = 1; j <= 2; j++)
                            {
                                if (j == 2)
                                {
                                    metricPercentage = 1 - metricPercentage;
                                }
                                double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);


                                if (!isCategory)
                                {
                                    barWorkbook.GetCell(0, i, j, tempMetricPercentage);
                                    tbl[chartCount + 1, i].TextFrame.Text = (Math.Round(Convert.ToDouble(compareData[index].Percentage * 100), 1).ToString("#0.0") + "%");
                                    tbl[chartCount + 1, i].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.FillType = FillType.Solid;
                                    tbl[chartCount + 1, i].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)compareData[index].Significance);
                                }
                                else
                                {
                                    barChart.Hidden = true;
                                    //tbl[chartCount + 1, i].TextFrame.Text = "NA";
                                    //tbl[chartCount + 1, i].TextFrame.Paragraphs[0].ParagraphFormat.Alignment = TextAlignment.Center;
                                    tbl[chartCount + 1, i].TextFrame.Text = "";
                                    cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("textBox" + (chartCount + 1) + "" + i)).Hidden = false;
                                }
                                /*IDataLabel lbl = barChart.ChartData.Series[j - 1].DataPoints[i - 1].Label;
                                lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                                lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)compareData[index].Significance);*/

                            }
                            index++;
                        }
                    }
                }
                chartCount++;
            }
            #endregion
        }
        // Method for binding slide 12 data:
        private void BindSlideThirteenData(List<PPTBindingData> pptData, ISlide cur_Slide, int numberOfCharts, int SlideNumber, List<ImagesList> imagesLists)
        {
            numberOfCharts++;
            var slideData = pptData.Where(x => x.SlideNumber == SlideNumber + 1).ToList();
            //slideData = slideData.OrderBy(a => a.GroupSort).ThenBy(a => a.SelectionType).ThenBy(a => a.SortId).ToList();
            var selectionList = slideData.Select(a => new { a.SelectionType, a.SelectionId, a.SelectionName, a.ConsumptionID, a.ConsumptionName }).Distinct().OrderBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ToList();
            int index = 0;
            int chartCount = 1;

            #region Slide 12 Image Replacement
            ITable tbl = (ITable)cur_Slide.Shapes.FirstOrDefault(x => x.Name == "Table 11");
            for (int i = 1; i <= numberOfCharts; i++)
            {
                if (selectionList[i - 1].ConsumptionID != 1)
                {
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName + ": " + selectionList[i - 1].ConsumptionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.Black;
                    format.FontBold = NullableBool.False;
                    format.FontHeight = (float)11;
                    format.LatinFont = new FontData("TCCC-UnityText");
                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }
                else if ((imagesLists.FindIndex(e => e.SelectionName == selectionList[i - 1].SelectionName) != -1) && (imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).HasImage))
                {
                    ((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i)).Hidden = false;
                    imageReplace((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i), imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).Image);
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.White;
                }
                else
                {
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.Black;
                    format.FontBold = NullableBool.False;
                    format.FontHeight = (float)11;
                    format.LatinFont = new FontData("TCCC-UnityText");
                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }
            }
            #endregion

            #region Slide 12 Data Binding
            var slideSections = SlideSections.twelfthSlideSections;
            int rowIndex = 2;
            for (int i = 1; i <= numberOfCharts; i++)
            {
                for (int j = 1; j <= 10; j++)
                {
                    if (!(new List<int> { 4, 9 }.Contains(j)))
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("textBox" + i + "" + j)).Hidden = true;
                }
            }
            foreach (var section in slideSections)
            {
                var sectionData = slideData.Where(x => x.MetricType == section).ToList();
                index = 0;
                for (int i = 1; i <= numberOfCharts; i++)
                {
                    int tempIndex = rowIndex;
                    string chartName = $"{section}{i}";
                    string chartNameBack = $"{section}{i}1";
                    IChart barChart = (IChart)cur_Slide.Shapes.FirstOrDefault(x => x.Name.ToLower() == chartName.ToLower());
                    IChart barChartBack = (IChart)cur_Slide.Shapes.FirstOrDefault(x => x.Name.ToLower() == chartNameBack.ToLower());
                    IChartDataWorkbook barWorkbook = barChart.ChartData.ChartDataWorkbook;
                    string selectionName = sectionData[index].SelectionName;
                    var benchamrkData = sectionData.Where(x => x.SelectionType == "Benchmark").ToList();
                    double metricPercentage = 0.0;
                    int dpCount = barChart.ChartData.Series[0].DataPoints.Count;
                    int seriesCount = barChart.ChartData.Series.Count;
                    barChart.TextFormat.ParagraphFormat.Alignment = TextAlignment.Right;

                    for (int j = 1; j <= dpCount; j++)
                    {
                        if ((j == 2 || j == 4) && section == "Consumption")
                        {
                            metricPercentage = (double)sectionData[index].Percentage;
                            if (j == 2)
                            {
                                var millionVal = metricPercentage.KiloMillionFormat();
                                tbl[i, 7].TextFrame.Text = millionVal;
                            }
                            else if (j == 4)
                            {
                                var millionVal = metricPercentage.KiloMillionFormat();
                                tbl[i, 9].TextFrame.Text = millionVal;
                            }
                        }
                        else
                        {
                            for (int k = 1; k <= seriesCount; k++)
                            {
                                metricPercentage = 0.0;
                                metricPercentage = (double)sectionData[index].Percentage;
                                double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);
                                if (k == 2)
                                {
                                    metricPercentage = 1 - metricPercentage;
                                    barWorkbook.GetCell(0, j, k, metricPercentage);
                                }
                                else
                                {

                                    bool isCategory = sectionData[index].IsCategory == 1 ? true : false;
                                    if (!isCategory || section == "Consumption")
                                    {
                                        barWorkbook.GetCell(0, j, k, tempMetricPercentage);
                                        tbl[i, tempIndex].TextFrame.Text = (Convert.ToDouble(sectionData[index].Percentage * 100).ToString("#0.0") + "%");
                                        tbl[i, tempIndex].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.FillType = FillType.Solid;
                                        tbl[i, tempIndex].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)sectionData[index].Significance);
                                    }
                                    else
                                    {
                                        barChart.Hidden = true;
                                        barChartBack.Hidden = true;
                                        //tbl[i, tempIndex].TextFrame.Text = "NA";
                                        //tbl[i, tempIndex].TextFrame.Paragraphs[0].ParagraphFormat.Alignment = TextAlignment.Center;
                                        tbl[i, tempIndex].TextFrame.Text = "";
                                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("textBox" + i + "" + (tempIndex - 1))).Hidden = false;
                                    }
                                }
                            }
                        }
                        tempIndex++;
                        index++;

                    }
                    chartCount++;
                    if (i == numberOfCharts)
                    {
                        rowIndex = tempIndex + 1;
                    }
                }
            }
            #endregion
        }
        // Method for binding slide 11 data:
        private void BindSlideElevenData(List<PPTBindingData> pptData, ISlide cur_Slide, int numberOfCharts, int SlideNumber, List<ImagesList> imagesLists)
        {
            int index = 0;
            int chartCount = 0;
            numberOfCharts++;
            var slideFourteenData = pptData.Where(x => x.SlideNumber == SlideNumber + 1).ToList();
            slideFourteenData = slideFourteenData.OrderBy(a => a.GroupSort).ThenBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ThenBy(a => a.SortId).ToList();
            var selectionList = slideFourteenData.Select(a => new { a.SelectionType, a.SelectionId, a.SelectionName, a.ConsumptionID, a.ConsumptionName }).Distinct().OrderBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ToList();

            #region Slide 14 Image Replacement
            ITable tbl = (ITable)cur_Slide.Shapes.FirstOrDefault(x => x.Name == "Table 11");
            for (int i = 1; i <= numberOfCharts; i++)
            {
                if (selectionList[i - 1].ConsumptionID != 1)
                {
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName + ": " + selectionList[i - 1].ConsumptionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.Black;
                    format.FontBold = NullableBool.False;
                    format.FontHeight = (float)11;
                    format.LatinFont = new FontData("TCCC-UnityText");
                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }
                else if ((imagesLists.FindIndex(e => e.SelectionName == selectionList[i - 1].SelectionName) != -1) && (imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).HasImage))
                {
                    ((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i)).Hidden = false;
                    imageReplace((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i), imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).Image);
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.White;
                }
                else
                {
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.Black;
                    format.FontBold = NullableBool.False;
                    format.FontHeight = (float)11;
                    format.LatinFont = new FontData("TCCC-UnityText");
                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }
            }
            #endregion

            #region Slide 14 Data Binding
            foreach (var item in selectionList)
            {
                string chartName = $"StackChart{chartCount + 1}";

                IChart barChart = (IChart)cur_Slide.Shapes.FirstOrDefault(x => x.Name.ToLower() == chartName.ToLower());
                IChartDataWorkbook barWorkbook = barChart.ChartData.ChartDataWorkbook;
                string selectionName = slideFourteenData[index].SelectionName;
                int selectionId = slideFourteenData[index].SelectionId;
                var benchamrkData = slideFourteenData.Where(x => x.SelectionType == "Benchmark").ToList();
                barChart.TextFormat.ParagraphFormat.Alignment = TextAlignment.Right;
                double metricPercentage = 0.0;
                int categoryCount = barChart.ChartData.Categories.Count;
                if (item.SelectionType == "Benchmark")
                {
                    // Benchmark data binding for slide 14:
                    for (int i = 1; i <= categoryCount; i++)
                    {
                        metricPercentage = 0.0;
                        metricPercentage = (double)benchamrkData[i - 1].Percentage;
                        for (int j = 1; j < 2; j++)
                        {
                            if (j == 2)
                            {
                                metricPercentage = 1 - metricPercentage;
                            }
                            double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);
                            barWorkbook.GetCell(0, i, j, tempMetricPercentage);
                            /*IDataLabel lbl = barChart.ChartData.Series[j - 1].DataPoints[i - 1].Label;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)benchamrkData[i - 1].Significance);*/
                            tbl[chartCount + 1, i].TextFrame.Text = (Convert.ToDouble(benchamrkData[i - 1].Percentage * 100).ToString("#0.0") + "%");
                            tbl[chartCount + 1, i].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.FillType = FillType.Solid;
                            tbl[chartCount + 1, i].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)benchamrkData[i - 1].Significance);
                        }
                    }
                }
                else
                {
                    var compareData = slideFourteenData.Where(x => x.SelectionType == "Comparison" && x.SelectionName == item.SelectionName && x.SelectionId == item.SelectionId).ToList();
                    for (int i = 1; i <= categoryCount; i++)
                    {
                        metricPercentage = 0.0;
                        metricPercentage = (double)compareData[i - 1].Percentage;
                        for (int j = 1; j < 2; j++)
                        {
                            if (j == 2)
                            {
                                metricPercentage = 1 - metricPercentage;
                            }
                            double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);
                            barWorkbook.GetCell(0, i, j, tempMetricPercentage);
                            /* IDataLabel lbl = barChart.ChartData.Series[j - 1].DataPoints[i - 1].Label;
                             lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                             lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)compareData[i - 1].Significance);*/
                            tbl[chartCount + 1, i].TextFrame.Text = (Convert.ToDouble(compareData[i - 1].Percentage * 100).ToString("#0.0") + "%");
                            tbl[chartCount + 1, i].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.FillType = FillType.Solid;
                            tbl[chartCount + 1, i].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)compareData[i - 1].Significance);
                        }
                    }
                }
                chartCount++;
            }
            #endregion
        }
        // Method for binding slide 14 data:
        private void BindSlideFifteenData(List<PPTBindingData> pptData, ISlide cur_Slide, int numberOfCharts, int SlideNumber, List<ImagesList> imagesLists)
        {
            int index = 0;
            int chartCount = 0;
            numberOfCharts++;
            var slideFourteenData = pptData.Where(x => x.SlideNumber == SlideNumber + 1).ToList();
            slideFourteenData = slideFourteenData.OrderBy(a => a.GroupSort).ThenBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ThenBy(a => a.SortId).ToList();
            var selectionList = slideFourteenData.Select(a => new { a.SelectionType, a.SelectionId, a.SelectionName, a.ConsumptionID, a.ConsumptionName }).Distinct().OrderBy(a => a.SelectionType).ThenBy(a => a.SelectionId).ToList();

            #region Slide 14 Image Replacement
            ITable tbl = (ITable)cur_Slide.Shapes.FirstOrDefault(x => x.Name == "Table 11");
            for (int i = 1; i <= numberOfCharts; i++)
            {
                if (selectionList[i - 1].ConsumptionID != 1)
                {
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName + ": " + selectionList[i - 1].ConsumptionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.Black;
                    format.FontBold = NullableBool.False;
                    format.FontHeight = (float)11;
                    format.LatinFont = new FontData("TCCC-UnityText");
                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }
                else if ((imagesLists.FindIndex(e => e.SelectionName == selectionList[i - 1].SelectionName) != -1) && (imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).HasImage))
                {
                    ((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i)).Hidden = false;
                    imageReplace((PictureFrame)cur_Slide.Shapes.FirstOrDefault(e => e.Name == "selection_" + i), imagesLists.Find(e => e.SelectionName == selectionList[i - 1].SelectionName).Image);
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.White;
                }
                else
                {
                    tbl[i, 0].TextFrame.Text = selectionList[i - 1].SelectionName;
                    IPortionFormat format = tbl[i, 0].TextFrame.Paragraphs[0].Portions[0].PortionFormat;
                    format.FillFormat.FillType = FillType.Solid;
                    format.FillFormat.SolidFillColor.Color = Color.Black;
                    format.FontBold = NullableBool.False;
                    format.FontHeight = (float)11;
                    format.LatinFont = new FontData("TCCC-UnityText");
                    if (true)
                    {
                        cur_Slide.Shapes.FirstOrDefault(x => x.Name == ("selection_" + i)).Hidden = true;
                    }
                }
            }
            #endregion

            #region Slide 14 Data Binding
            foreach (var item in selectionList)
            {
                string chartName = $"StackChart{chartCount + 1}";

                IChart barChart = (IChart)cur_Slide.Shapes.FirstOrDefault(x => x.Name.ToLower() == chartName.ToLower());
                IChartDataWorkbook barWorkbook = barChart.ChartData.ChartDataWorkbook;
                string selectionName = slideFourteenData[index].SelectionName;
                int selectionId = slideFourteenData[index].SelectionId;
                var benchamrkData = slideFourteenData.Where(x => x.SelectionType == "Benchmark").ToList();
                barChart.TextFormat.ParagraphFormat.Alignment = TextAlignment.Right;
                double metricPercentage = 0.0;
                int categoryCount = barChart.ChartData.Categories.Count;
                if (item.SelectionType == "Benchmark")
                {
                    // Benchmark data binding for slide 14:
                    for (int i = 1; i <= categoryCount; i++)
                    {
                        metricPercentage = 0.0;
                        metricPercentage = (double)benchamrkData[i - 1].Percentage;
                        for (int j = 1; j <= 2; j++)
                        {
                            if (j == 2)
                            {
                                metricPercentage = 1 - metricPercentage;
                            }
                            double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);
                            barWorkbook.GetCell(0, i, j, tempMetricPercentage);
                            /*IDataLabel lbl = barChart.ChartData.Series[j - 1].DataPoints[i - 1].Label;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                            lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)benchamrkData[i - 1].Significance);*/
                            tbl[chartCount + 1, i].TextFrame.Text = (Convert.ToDouble(benchamrkData[i - 1].Percentage * 100).ToString("#0.0") + "%");
                            tbl[chartCount + 1, i].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.FillType = FillType.Solid;
                            tbl[chartCount + 1, i].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)benchamrkData[i - 1].Significance);
                        }
                    }
                }
                else
                {
                    var compareData = slideFourteenData.Where(x => x.SelectionType == "Comparison" && x.SelectionName == item.SelectionName && x.SelectionId == item.SelectionId).ToList();
                    for (int i = 1; i <= categoryCount; i++)
                    {
                        metricPercentage = 0.0;
                        metricPercentage = (double)compareData[i - 1].Percentage;
                        for (int j = 1; j <= 2; j++)
                        {
                            if (j == 2)
                            {
                                metricPercentage = 1 - metricPercentage;
                            }
                            double tempMetricPercentage = Convert.ToDouble(metricPercentage > 1 ? metricPercentage / 100 : metricPercentage);
                            barWorkbook.GetCell(0, i, j, tempMetricPercentage);
                            /* IDataLabel lbl = barChart.ChartData.Series[j - 1].DataPoints[i - 1].Label;
                             lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.FillType = FillType.Solid;
                             lbl.DataLabelFormat.TextFormat.PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)compareData[i - 1].Significance);*/
                            tbl[chartCount + 1, i].TextFrame.Text = (Convert.ToDouble(compareData[i - 1].Percentage * 100).ToString("#0.0") + "%");
                            tbl[chartCount + 1, i].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.FillType = FillType.Solid;
                            tbl[chartCount + 1, i].TextFrame.Paragraphs[0].Portions[0].PortionFormat.FillFormat.SolidFillColor.Color = getSigcolorNormal((double)compareData[i - 1].Significance);
                        }
                    }
                }
                chartCount++;
            }
            #endregion
        }
        // Method for generating data table for filter:
        private System.Data.DataTable GetTableOutputPPT(List<PopupLevelData> selection, string stubType, System.Data.DataTable table, string filterType)
        {
            if (filterType.Equals("Filter", StringComparison.OrdinalIgnoreCase))
            {
                table.Columns.Add("Attributegroupid", typeof(Int32));
                table.Columns.Add("Attributeid", typeof(Int32));
                foreach (var item in selection)
                {
                    DataRow demogsRow = table.NewRow();
                    string[] parentMap = item.parentMap.Split(",");
                    if (item.type.Equals("Filter", StringComparison.OrdinalIgnoreCase))
                    {
                        demogsRow[0] = Convert.ToInt32(parentMap[1]);
                        demogsRow[1] = Convert.ToInt32(parentMap[2]);
                        table.Rows.Add(demogsRow);
                    }
                }
            }
            else if (filterType.Equals("BeverageFilter", StringComparison.OrdinalIgnoreCase))
            {
                table.Columns.Add("CategoryID", typeof(Int32));
                table.Columns.Add("LowLevelCategoryID", typeof(Int32));
                table.Columns.Add("TrademarkID", typeof(Int32));
                table.Columns.Add("BrandID", typeof(Int32));
                table.Columns.Add("ConsumptionID", typeof(Int32));
                foreach (var item in selection)
                {
                    DataRow beveragesRow = table.NewRow();
                    string[] parentMap = item.parentMap.Split(",");
                    if (item.type.Equals("BeverageFilter", StringComparison.OrdinalIgnoreCase))
                    {
                        beveragesRow[0] = Convert.ToInt32(parentMap[1]);

                        if (item.levelId >= 3)
                        {
                            beveragesRow[1] = Convert.ToInt32(parentMap[2]);
                        }
                        else
                        {
                            beveragesRow[1] = DBNull.Value;
                        }
                        if (item.levelId == 4)
                        {
                            beveragesRow[2] = Convert.ToInt32(item.metricId);
                        }
                        else
                        {
                            beveragesRow[2] = DBNull.Value;
                        }
                        if (item.levelId == 5)
                        {
                            beveragesRow[3] = Convert.ToInt32(item.metricId);
                        }
                        else
                        {
                            beveragesRow[3] = DBNull.Value;
                        }
                        beveragesRow[4] = item.consumptionId;
                        table.Rows.Add(beveragesRow);
                    }
                }
            }
            return table;
        }
        // Method for generating data table for geography/benchmark/comaprison:
        private System.Data.DataTable GetTableOutputPPT(List<PopupLevelData> selection, string stubType, System.Data.DataTable table)
        {
            if (stubType.Equals("GeographyMenu", StringComparison.OrdinalIgnoreCase))
            {
                table.Columns.Add("OUID", typeof(Int32));
                table.Columns.Add("CountryID", typeof(Int32));
                table.Columns.Add("RegionID", typeof(Int32));
                table.Columns.Add("BottlerID", typeof(Int32));

                foreach (var item in selection)
                {
                    DataRow geographyRow = table.NewRow();
                    if (item.type.Equals("Total", StringComparison.OrdinalIgnoreCase))
                    {
                        geographyRow[0] = -1;
                        geographyRow[1] = -1;
                        geographyRow[2] = -1;
                        geographyRow[3] = -1;
                    }
                    else if (item.type.Equals("OU", StringComparison.OrdinalIgnoreCase))
                    {
                        geographyRow[0] = Convert.ToInt32(item.metricId);
                        geographyRow[1] = DBNull.Value;
                        geographyRow[2] = DBNull.Value;
                        geographyRow[3] = DBNull.Value;
                    }
                    else if (item.type.Equals("Country", StringComparison.OrdinalIgnoreCase))
                    {
                        geographyRow[0] = DBNull.Value;
                        geographyRow[1] = Convert.ToInt32(item.metricId);
                        geographyRow[2] = DBNull.Value;
                        geographyRow[3] = DBNull.Value;
                    }
                    else if (item.type.Equals("Region", StringComparison.OrdinalIgnoreCase))
                    {
                        geographyRow[0] = DBNull.Value;
                        geographyRow[1] = Convert.ToInt32(item.parentMetricId);
                        geographyRow[2] = Convert.ToInt32(item.metricId);
                        geographyRow[3] = DBNull.Value;
                    }
                    else
                    {
                        geographyRow[0] = DBNull.Value;
                        geographyRow[1] = Convert.ToInt32(item.parentMetricId);
                        geographyRow[2] = DBNull.Value;
                        geographyRow[3] = Convert.ToInt32(item.metricId);
                    }
                    table.Rows.Add(geographyRow);
                }
            }
            else if (stubType.Equals("Benchmark", StringComparison.OrdinalIgnoreCase) || stubType.Equals("Comparison", StringComparison.OrdinalIgnoreCase))
            {
                int index = 0;
                if (stubType.Equals("Benchmark", StringComparison.OrdinalIgnoreCase))
                {
                    index = 100;
                }
                else
                {
                    index = 200;
                }

                table.Columns.Add("CategoryID", typeof(Int32));
                table.Columns.Add("LowLevelCategoryID", typeof(Int32));
                table.Columns.Add("TrademarkID", typeof(Int32));
                table.Columns.Add("BrandID", typeof(Int32));
                table.Columns.Add("ConsumptionID", typeof(Int32));
                table.Columns.Add("SelectionType", typeof(string));
                table.Columns.Add("SelectionID", typeof(Int32));
                table.Columns.Add("SelectionName", typeof(string));

                if (selection.Count > 0)
                {
                    foreach (var item in selection)
                    {
                        string[] parentMap = item.parentMap.Split(",");
                        DataRow benchmarkRow = table.NewRow();
                        benchmarkRow[0] = Convert.ToInt32(parentMap[0]);
                        if (1 < parentMap.Length)
                        {
                            benchmarkRow[1] = Convert.ToInt32(parentMap[1]);
                        }
                        else
                        {
                            benchmarkRow[1] = DBNull.Value;
                        }

                        benchmarkRow[4] = Convert.ToInt32(item.consumptionId);
                        benchmarkRow[5] = stubType;
                        //benchmarkRow[6] = Convert.ToInt32(item.id);
                        benchmarkRow[6] = Convert.ToInt32(index);
                        benchmarkRow[7] = item.text;
                        if (item.type.Equals("Trademark", StringComparison.OrdinalIgnoreCase))
                        {
                            benchmarkRow[2] = Convert.ToInt32(item.metricId);
                            benchmarkRow[3] = DBNull.Value;
                        }
                        else if (item.type.Equals("Brand", StringComparison.OrdinalIgnoreCase))
                        {
                            benchmarkRow[2] = DBNull.Value;
                            benchmarkRow[3] = Convert.ToInt32(item.metricId);
                        }
                        table.Rows.Add(benchmarkRow);
                        index++;
                    }
                }
            }
            return table;
        }
        // Method for deleting the slides which are not required:
        private void DeleteSlides(int selectionLength, Presentation pres)
        {
            SortedSet<int> slidesNeeded = new SortedSet<int>
            {
                0,
                1,
                2,
                63,
                70
            };

            int length = 6 - selectionLength;
            int startSlideindex = 3 + length;

            while (startSlideindex < 63)
            {
                slidesNeeded.Add(startSlideindex);
                startSlideindex += 6;
            }

            /* startSlideindex = 28;
             startSlideindex += length;
             while (startSlideindex < 52)
             {
                 slidesNeeded.Add(startSlideindex);
                 startSlideindex += 6;
             }*/

            startSlideindex = 64;
            startSlideindex += length;
            while (startSlideindex < 71)
            {
                slidesNeeded.Add(startSlideindex);
                startSlideindex += 6;
            }

            for (int i = pres.Slides.Count - 2; i >= 0; i--)
            {
                if (!slidesNeeded.Contains(i))
                {
                    pres.Slides.Remove(pres.Slides[i]);
                }
            }
        }
        // Method for setting data label color according to their significance value:
        private Color getSigcolorNormal(double? cbsig)
        {
            if (cbsig == 0.0) return Color.Black;
            if (cbsig < -1.96) return Color.Red;
            if (cbsig > 1.96) return Color.Green;
            return Color.Black;
        }
        // Method for replacing images:
        public void imageReplace(PictureFrame tempImg, Image img)
        {
            tempImg.PictureFormat.Picture.Image.ReplaceImage(img);
        }
        // Method for getting color for :
        private string GetColor(string metric)
        {
            Dictionary<string, string> colorCode = new Dictionary<string, string>()
            {
                {"eating", "#FF0000" },
                {"Leisure","#7A0005" },
                {"On the move","#FF560E" },
                {"Related to School/Training","#FF9A6E" },
                {"Routine or everyday activity","#FFC361" },
                {"Socializing","#B59E7A" },
                {"Sports or physical activity","#6ACE7F" },
                {"Working","#277537" },
                {"Others","#6AC9CE" }
            };

            if (colorCode.ContainsKey(metric) == true)
            {
                return colorCode[metric];
            }
            return "FF0000";
        }
    }
}

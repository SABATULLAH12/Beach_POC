using coke_beach_reportGenerator_api.Models;
using coke_beach_reportGenerator_api.Models.LeftPanelModel;
using coke_beach_reportGenerator_api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace coke_beach_reportGenerator_api.Services
{
    public class ReportGeneratorService : IReportGeneratorService
    {
        public ReportGeneratorService()
        {
            //TODO:
        }
        // Method for fetching data from db to check the sample size:
        public List<SampleSizeModel> CheckSampleSize(DataTable geoTable, long timePeriodId, DataTable benchmarkCompareTable, DataTable demogsTable, DataTable beveragesTable)
        {
            List<SampleSizeModel> sampleSizeList = new List<SampleSizeModel>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SPSamplesize ", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };
                command.Parameters.Add(new SqlParameter("@GeoSelections", geoTable));
                command.Parameters.Add(new SqlParameter("@timeperiodid", timePeriodId));
                command.Parameters.Add(new SqlParameter("@BenchmarkComparisonSelections", benchmarkCompareTable));
                command.Parameters.Add(new SqlParameter("@DemographySelections", demogsTable));
                command.Parameters.Add(new SqlParameter("@BeverageSelections", beveragesTable));

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    SampleSizeModel sampleSize = new SampleSizeModel()
                    {
                        SelectionID = Convert.ToInt32(reader["SelectionID"]),
                        SelectionType = reader["SelectionType"].ToString(),
                        SelectionName = reader["SelectionName"].ToString(),
                        ConsumptionId = Convert.ToInt32(reader["ConsumptionId"]),
                        SampleSize = Convert.ToInt32(reader["SampleSize"])
                    };
                    sampleSizeList.Add(sampleSize);
                }
            }
            return sampleSizeList;
        }
        // Method for fetching data from db to bind data into the ppt:
        public List<PPTBindingData> GetPPTBindingData(string spName, DataTable geoTable, long timePeriodId, DataTable benchmarkCompareTable, DataTable demogsTable, DataTable beveragesTable)
        {
            List<PPTBindingData> pptData = new List<PPTBindingData>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(spName, connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };
                command.Parameters.Add(new SqlParameter("@GeoSelections", geoTable));
                command.Parameters.Add(new SqlParameter("@timeperiodid", timePeriodId));
                command.Parameters.Add(new SqlParameter("@BenchmarkComparisonSelections", benchmarkCompareTable));
                command.Parameters.Add(new SqlParameter("@DemographySelections", demogsTable));
                command.Parameters.Add(new SqlParameter("@BeverageSelections", beveragesTable));

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    PPTBindingData pptBindingData = new PPTBindingData()
                    {
                        SelectionType = reader["SelectionType"].ToString(),
                        MetricType = reader["MetricType"].ToString(),
                        Metric = reader["Metric"].ToString(),
                        SelectionName = reader["SelectionName"].ToString(),
                        Percentage = Convert.ToDecimal(reader["Percentage"]),
                        Significance = Convert.ToDecimal(reader["Significance"]),
                        GroupSort = Convert.ToInt32(reader["GroupSort"]),
                        SortId = Convert.ToInt32(reader["SortId"]),
                        SlideNumber = Convert.ToInt32(reader["SlideNumber"]),
                        SelectionId = Convert.ToInt32(reader["SelectionId"]),
                        IsCategory = Convert.ToInt32(reader["IsCategory"]),
                        ConsumptionID = Convert.ToInt32(reader["ConsumptionID"]),
                        ConsumptionName = reader["ConsumptionName"].ToString(),
                    };
                    pptData.Add(pptBindingData);
                }
            }
            return pptData;
        }

        public List<PPTBindingData> GetDuumyPPTBindingData(DataTable geoTable, long timePeriodId, DataTable benchmarkCompareTable, DataTable demogsTable, DataTable beveragesTable)
        {
            List<PPTBindingData> pptData = new List<PPTBindingData>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT * FROM LASTSLIDEDUMMYDATA", connection)
                {
                    CommandTimeout = 7200
                };
               /* command.Parameters.Add(new SqlParameter("@GeoSelections", geoTable));
                command.Parameters.Add(new SqlParameter("@timeperiodid", timePeriodId));
                command.Parameters.Add(new SqlParameter("@BenchmarkComparisonSelections", benchmarkCompareTable));
                command.Parameters.Add(new SqlParameter("@DemographySelections", demogsTable));
                command.Parameters.Add(new SqlParameter("@BeverageSelections", beveragesTable));*/

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    PPTBindingData pptBindingData = new PPTBindingData()
                    {
                        SelectionType = reader["SelectionType"].ToString(),
                        MetricType = reader["MetricType"].ToString(),
                        Metric = reader["Metric"].ToString(),
                        SelectionName = reader["SelectionName"].ToString(),
                        Percentage = Convert.ToDecimal(reader["Percentage"]),
                        Significance = Convert.ToDecimal(reader["Significance"]),
                        GroupSort = Convert.ToInt32(reader["GroupSort"]),
                        SortId = Convert.ToInt32(reader["SortId"]),
                        SlideNumber = Convert.ToInt32(reader["SlideNumber"])
                    };
                    pptData.Add(pptBindingData);
                }
            }
            return pptData;
        }

        // Method for fetching data from db to get image mapping information to replace in the slides:
        public List<ImagesList> GetImagesData(DataTable benchmarkCompareTable)
        {
            List<ImagesList> _images = new List<ImagesList>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("GetImagesName", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };
                command.Parameters.Add(new SqlParameter("@BenchmarkComparisonSelections", benchmarkCompareTable));

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ImagesList image = new ImagesList()
                    {
                        SelectionName = reader["SelectionName"].ToString(),
                        ImageName = reader["ImagesName"].ToString(),
                        HasImage = Convert.ToBoolean(reader["HasImage"]),
                        IsText = Convert.ToBoolean(reader["IsText"])
                    };
                    _images.Add(image);
                }
            }
            return _images;
        }

        public List<PPTBindingData> GetPPTBindingDataDummy(DataTable geoTable, long timePeriodId, DataTable benchmarkCompareTable, DataTable demogsTable, DataTable beveragesTable)
        {
            List<PPTBindingData> pptData = new List<PPTBindingData>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("Select * from MultiSelectDummyStaticData", connection)
                {
                    CommandTimeout = 7200
                };
               /* command.Parameters.Add(new SqlParameter("@GeoSelections", geoTable));
                command.Parameters.Add(new SqlParameter("@timeperiodid", timePeriodId));
                command.Parameters.Add(new SqlParameter("@BenchmarkComparisonSelections", benchmarkCompareTable));
                command.Parameters.Add(new SqlParameter("@DemographySelections", demogsTable));
                command.Parameters.Add(new SqlParameter("@BeverageSelections", beveragesTable));*/

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    PPTBindingData pptBindingData = new PPTBindingData()
                    {
                        SelectionType = reader["SelectionType"].ToString(),
                        MetricType = reader["MetricType"].ToString(),
                        Metric = reader["Metric"].ToString(),
                        SelectionName = reader["SelectionName"].ToString(),
                        Percentage = Convert.ToDecimal(reader["Percentage"]),
                        Significance = Convert.ToDecimal(reader["Significance"]),
                        GroupSort = Convert.ToInt32(reader["GroupSort"]),
                        SortId = Convert.ToInt32(reader["SortId"]),
                        SlideNumber = Convert.ToInt32(reader["SlideNumber"])
                    };
                    pptData.Add(pptBindingData);
                }
            }
            return pptData;
        }
        public PPTResponse GetStatusFromDB(Guid guid)
        {
            PPTResponse ppt = null;
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("Get_Poll_Status", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };
                command.Parameters.Add(new SqlParameter("@Id", guid.ToString()));
                object k = null;

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ppt = new PPTResponse();
                    ppt.id = Guid.Parse(reader["Id"].ToString());
                    ppt.isCompleted = Convert.ToBoolean(reader["IsCompleted"]);
                    ppt.errorMessage = reader["ErrorMessage"].ToString();
                    ppt.isError = Convert.ToBoolean(reader["IsError"]);
                    k = reader["Data"];
                }
                if (ppt.isCompleted)
                {
                    ppt.data = (byte[])k;
                }
            }
            return ppt;
        }
        public void InsertStatusInDB(PPTResponse ppt)
        {
            DataTable input = new DataTable();
            input.Columns.Add("Id", typeof(string));
            input.Columns.Add("IsCompleted", typeof(bool));
            input.Columns.Add("ErrorMessage", typeof(string));
            input.Columns.Add("IsError", typeof(bool));
            input.Columns.Add("Data", typeof(byte[]));
            DataRow row = input.NewRow();
            row[0] = ppt.id;
            row[1] = ppt.isCompleted;
            row[2] = ppt.errorMessage;
            row[3] = ppt.isError;
            row[4] = ppt.data;
            input.Rows.Add(row);
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("Insert_Poll_Status", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };
                command.Parameters.Add(new SqlParameter("@Selection", input));

                int numOfRows = command.ExecuteNonQuery();
                
            }
            
        }
        public void UpdateStatusInDB(PPTResponse ppt)
        {
            DataTable input = new DataTable();
            input.Columns.Add("Id", typeof(string));
            input.Columns.Add("IsCompleted", typeof(bool));
            input.Columns.Add("ErrorMessage", typeof(string));
            input.Columns.Add("IsError", typeof(bool));
            input.Columns.Add("Data", typeof(byte[]));
            DataRow row = input.NewRow();
            row[0] = ppt.id;
            row[1] = ppt.isCompleted;
            row[2] = ppt.errorMessage;
            row[3] = ppt.isError;
            row[4] = ppt.data;
            input.Rows.Add(row);
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("Update_Poll_Status", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };
                command.Parameters.Add(new SqlParameter("@Selection", input));

                int numOfRows = command.ExecuteNonQuery();

            }

        }
        public void DeleteStatusFromDB(Guid guid)
        {
            
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("Delete_Poll_Status", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };
                command.Parameters.Add(new SqlParameter("@Id", guid.ToString()));

                int numOfRows = command.ExecuteNonQuery();

            }

        }
    }
}

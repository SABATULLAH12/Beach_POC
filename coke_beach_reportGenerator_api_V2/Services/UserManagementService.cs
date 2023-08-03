using coke_beach_reportGenerator_api.Models.UserManagementModels;
using coke_beach_reportGenerator_api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace coke_beach_reportGenerator_api.Services
{
    public class UserManagementService : IUserManagementService
    {
        public int AddUsers(string Name, string Email, string Location)
        {
            int rowCount = 0;

            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SPUSM_AddUser", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };
                command.Parameters.Add(new SqlParameter("@name", Name));
                command.Parameters.Add(new SqlParameter("@email", Email));
                //command.Parameters.Add(new SqlParameter("@location", Location));
                rowCount = command.ExecuteNonQuery();
            }
            return rowCount;
        }

        public int AddUserSelectionStat(string Email, string Country, string TimePeriod, string Benchmark, string Comparison, string Filter)
        {
            int rowCount = 0;

            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SPUSM_DUser_AddSelection", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };

                command.Parameters.Add(new SqlParameter("@email", Email));
                command.Parameters.Add(new SqlParameter("@geo", Country));
                command.Parameters.Add(new SqlParameter("@timePeriod", TimePeriod));
                command.Parameters.Add(new SqlParameter("@Benchmark", Benchmark));
                command.Parameters.Add(new SqlParameter("@comparision", Comparison));
                command.Parameters.Add(new SqlParameter("@filters", Filter));

                rowCount = command.ExecuteNonQuery();
            }
            return rowCount;
        }

        public List<ReportCountModel> GetReportCount(string RoleId, int TimePeriodId)
        {
            List<ReportCountModel> reportCounts = new List<ReportCountModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("SPUSM_ReportCount", connection)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandTimeout = 7200
                    };

                    if (string.IsNullOrEmpty(RoleId))
                    {
                        command.Parameters.Add(new SqlParameter("@role", DBNull.Value));
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@role", RoleId));
                    }

                    if (TimePeriodId == -1)
                    {
                        command.Parameters.Add(new SqlParameter("@TimeperiodID", DBNull.Value));
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@TimeperiodID", TimePeriodId));
                    }

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ReportCountModel reportCount = new ReportCountModel()
                        {
                            Name = reader["Name"].ToString(),
                            EmailId = reader["emailid"].ToString(),
                            ReportDownloadCount = Convert.ToInt32(reader["ReportDownloaded"]),
                            Role = reader["role"].ToString(),
                        };
                        reportCounts.Add(reportCount);
                    }
                }
            }
            catch (Exception e)
            {

            }
            return reportCounts;
        }

        public List<ReportSelectionDetailModel> GetReportSelectionDetails(string RoleId, int TimePeriodId)
        {
            List<ReportSelectionDetailModel> selectionDetails = new List<ReportSelectionDetailModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("SPUSM_ReportSelectionDetails", connection)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandTimeout = 7200
                    };

                    if (string.IsNullOrEmpty(RoleId))
                    {
                        command.Parameters.Add(new SqlParameter("@role", DBNull.Value));
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@role", RoleId));
                    }

                    if (TimePeriodId == -1)
                    {
                        command.Parameters.Add(new SqlParameter("@TimeperiodID", DBNull.Value));
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@TimeperiodID", TimePeriodId));
                    }

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ReportSelectionDetailModel selectionDetail = new ReportSelectionDetailModel()
                        {
                            SrNo = reader["Sr No"].ToString(),
                            Name = reader["Name"].ToString(),
                            EmailId = reader["EmailID"].ToString(),
                            DownloadDate = reader["DownloadDate"].ToString(),
                            Role = reader["Role"].ToString(),
                            Geography = reader["Geography"].ToString(),
                            TimePeriod = reader["TimePeriod"].ToString(),
                            Benchmark = reader["Benchmark"].ToString(),
                            Comparison = reader["Comparision"].ToString(),
                            Filter = reader["Filters"].ToString()
                        };
                        selectionDetails.Add(selectionDetail);
                    }
                }
            }
            catch (Exception e)
            {

            }
            return selectionDetails;
        }

        public List<GetUserManagementModel> GetUserDetails()
        {
            List<GetUserManagementModel> userDetails = new List<GetUserManagementModel>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT * FROM USM_USERDATA", connection)
                {
                    CommandTimeout = 7200
                };

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GetUserManagementModel userDetail = new GetUserManagementModel()
                    {
                        SrNo = Convert.ToInt32(reader["SrNo"]),
                        Name = reader["Name"].ToString(),
                        EmailId = reader["EmailId"].ToString(),
                        Date = reader["Date"].ToString(),
                        Role = reader["Role"].ToString(),
                    };
                    userDetails.Add(userDetail);
                }
            }
            return userDetails;
        }

        public DataSet GetUSMLeftPanel()
        {
            DataSet dataSet = new DataSet();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                dataSet = new DataSet();
                SqlCommand command = new SqlCommand("SPUSM_LeftPanel", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                try
                {
                    adapter.Fill(dataSet);
                }
                catch (Exception ex)
                {
                    dataSet = null;
                }
                return dataSet;
            }
        }

        public int UpdateUsers(string Email, string Role)
        {
            int rowCount = 0;

            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SPUSM_EditUser", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };
                command.Parameters.Add(new SqlParameter("@email", Email));
                command.Parameters.Add(new SqlParameter("@Role", Role));
                rowCount = command.ExecuteNonQuery();
            }
            return rowCount;
        }
        public DataSet GetDataAvailability()
        {
            DataSet dataSet = new DataSet();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                dataSet = new DataSet();
                SqlCommand command = new SqlCommand("SPCountryLatestTimPeriod", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                try
                {
                    adapter.Fill(dataSet);
                }
                catch (Exception ex)
                {
                    dataSet = null;
                }
                return dataSet;
            }
        }

        public DataSet GetPBIDashboard()
        {
            DataSet dataSet = new DataSet();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                dataSet = new DataSet();
                SqlCommand command = new SqlCommand("SPCountryLatestTimPeriod", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                try
                {
                    adapter.Fill(dataSet);
                }
                catch (Exception ex)
                {
                    dataSet = null;
                }
                return dataSet;
            }
        }
    }
}

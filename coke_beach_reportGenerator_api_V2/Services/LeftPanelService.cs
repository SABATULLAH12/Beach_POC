using coke_beach_reportGenerator_api.Models;
using coke_beach_reportGenerator_api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coke_beach_reportGenerator_api.Services
{
    public class LeftPanelService : ILeftPanelService
    {
        private DataSet dataSet;
        // private SqlDatabase dataBase;
        private DbCommand sqlCommand;
        public string connString { get; private set; }
        public int timeOut { get; private set; }
        public LeftPanelService()
        {
            //TODO:
        }

        public DataSet GetData(string spName, string countryId)
        {
            dataSet = new DataSet();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                dataSet = new DataSet();
                SqlCommand command = new SqlCommand(spName, connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };
                command.Parameters.Add(new SqlParameter("@selection", countryId));
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

        public DataSet GetData(string spName)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                dataSet = new DataSet();
                SqlCommand cmd = new SqlCommand(spName, connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 7200
                };
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
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

        public int SaveData(string spName, object[] parameter)
        {
            return 1;
        }
    }
}

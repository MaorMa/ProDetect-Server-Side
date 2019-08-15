using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace RRS_API.Models
{

    public class AzureConnection
    {
        private SqlConnectionStringBuilder builder;
        private static AzureConnection INSTANCE = new AzureConnection();

        /*
         * create connection to azure database
         * implements singelton design pattern
         */
        private AzureConnection()
        {
            try
            {
                builder = new SqlConnectionStringBuilder();
                builder.DataSource = "rrsystem.database.windows.net";
                builder.UserID = "yanivmaor";
                builder.Password = "yanIv021193";
                builder.InitialCatalog = "RRS";
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static AzureConnection getInstance()
        {
            return INSTANCE;
        }

        /*
         * abstract select query from db
         * query - a given query
         * return List contains query result
         * seperated by comma 
         */
        public List<String> SelectQuery(String query) {
            List<String> result = new List<string>();
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                if(!(connection.State == System.Data.ConnectionState.Open))
                {
                    connection.Open();
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(query);
                String sql = sb.ToString();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            String tmpResult = "";
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                if (i == 0)
                                {
                                    tmpResult += reader[i];
                                }
                                else
                                {
                                    tmpResult += "," + reader[i];
                                }
                            }

                            result.Add(tmpResult);
                        }
                    }
                }
            }
            return result;
        }

        internal string[] getName(string key, string selectedMarket)
        {
            throw new NotImplementedException();
        }

        /*
         * update receipt status by resercher
         * change status from NO to YES
         */
        public void updateStatus(String receiptName)
        {

        }

        /*
         * insert data for a given family
         * after resercher approved results from ocr
         */ 
        public Boolean saveFamilyData(String familiyID, List<String> data)
        {
            Boolean success = false;
            return success;
        }
    }
}
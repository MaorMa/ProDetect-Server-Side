﻿using ImageRecognition.Objects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
                builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["AzureConnection"].ToString());
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
         * generic select query 
         * return List contains query result
         * seperated by comma 
         */
        public List<String> SelectQuery(String query)
        {
            List<String> result = new List<string>();
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                if (!(connection.State == System.Data.ConnectionState.Open))
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

        /*
         * after researcher approved a receipt
         * change status from 0 to 1 
         * 0 - means need to be approved , 1 - means already approved 
         */
        public void updateStatus(String receiptID)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                if (!(connection.State == System.Data.ConnectionState.Open))
                {
                    connection.Open();
                }
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "UPDATE FamilyUploads SET ReceiptStatus = 1 Where ReceiptID = @receiptID";
                    command.Parameters.AddWithValue("@ReceiptID", receiptID);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException sqlException)
                    {
                        throw sqlException;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        /*
         * if researcher make changes 
         */
        public void updateReceiptData(string selectedFamilyID, Dictionary<string, List<MetaData>>.ValueCollection values)
        {
            throw new NotImplementedException();
        }

        /*
         * insert data for a given family
         */
        public void updateFamilyUploads(string selectedFamilyID, string MarketID, string imageName, int status)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                if (!(connection.State == System.Data.ConnectionState.Open))
                {
                    connection.Open();
                }
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "INSERT into FamilyUploads(FamilyID, MarketID, ReceiptID, ReceiptStatus) VALUES(@FamilyID, @MarketID, @ReceiptID, @ReceiptStatus)";
                    command.Parameters.AddWithValue("@FamilyID", selectedFamilyID);
                    command.Parameters.AddWithValue("@MarketID", MarketID);
                    command.Parameters.AddWithValue("@ReceiptID", imageName);
                    command.Parameters.AddWithValue("@ReceiptStatus", status);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException sqlException)
                    {
                        throw sqlException;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public void insertReceiptData(string receiptID, string productID, string productDescription, string productQuantity, string productPrice, double yCoordinate)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                if (!(connection.State == System.Data.ConnectionState.Open))
                {
                    connection.Open();
                }
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "INSERT into ReceiptData(ReceiptID, ProductID, Description, Quantity, Price, YCoordinate) VALUES(@ReceiptID, @ProductID, @Description, @Quantity, @Price, @YCoordinate)";
                    command.Parameters.AddWithValue("@ReceiptID", receiptID);
                    command.Parameters.AddWithValue("@ProductID", productID);
                    command.Parameters.AddWithValue("@Description", productDescription);
                    command.Parameters.AddWithValue("@Quantity", productQuantity);
                    command.Parameters.AddWithValue("@Price", productPrice);
                    command.Parameters.AddWithValue("@YCoordinate", yCoordinate.ToString());
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException sqlException)
                    {
                        throw sqlException;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }


        /*
         * delete products of given receipt
         */
        public void deleteReceiptData(string receiptID)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                if (!(connection.State == System.Data.ConnectionState.Open))
                {
                    connection.Open();
                }
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "DELETE FROM ReceiptData WHERE ReceiptID = @ReceiptID";
                    command.Parameters.AddWithValue("@ReceiptID", receiptID);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException sqlException)
                    {
                        throw sqlException;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

    }
}
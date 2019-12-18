using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using log4net;
using System.Reflection;

namespace RRS_API.Models
{

    public class AzureConnection
    {
        private SqlConnectionStringBuilder builder;
        private static AzureConnection INSTANCE = new AzureConnection();
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /*
         * create connection to azure database
         * implements singelton design pattern
         */
        private AzureConnection()
        {
            try
            {
                _logger.Debug("Trying to create connection to SQL Server");
                builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["AzureConnection"].ToString());
                _logger.Info("Succesful connection to SQL Server");
            }
            catch (Exception e)
            {
                //_logger.Error($"Error while connecting to SQL Server", e);
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
            //_logger.Debug($"Running select query {query}");
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
        public void updateStatus(string FamilyID, string ReceiptID, string status)
        {
            _logger.Debug($"Updating receipt status, familyID: {FamilyID}, ReceiptID: {ReceiptID}, status: {status}");
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
                    command.CommandText = "UPDATE FamilyUploads SET ReceiptStatus = " + status + " Where FamilyID = @FamilyID AND ReceiptID = @ReceiptID";
                    command.Parameters.AddWithValue("@ReceiptID", ReceiptID);
                    command.Parameters.AddWithValue("@FamilyID", FamilyID);
                    try
                    {
                        command.ExecuteNonQuery();
                        _logger.Info($"Succesful Updating receipt status, familyID: {FamilyID}, ReceiptID: {ReceiptID}");
                    }
                    catch (SqlException sqlException)
                    {
                        _logger.Error($"Error while Updating receipt status, familyID: {FamilyID}, ReceiptID: {ReceiptID}", sqlException);
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
         * insert data for a given family
         */
        public void updateFamilyUploads(string selectedFamilyID, string MarketID, string imageName, int status, string UploadTime)
        {
            _logger.Debug($"Updating family uploads, familyID: {selectedFamilyID}, MarketID: {MarketID}, imageName: {imageName} status: {status}, upload time: {UploadTime}");
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
                    command.CommandText = "INSERT into FamilyUploads(FamilyID, MarketID, ReceiptID, ReceiptStatus, UploadTime) VALUES(@FamilyID, @MarketID, @ReceiptID, @ReceiptStatus,@UploadTime)";
                    command.Parameters.AddWithValue("@FamilyID", selectedFamilyID);
                    command.Parameters.AddWithValue("@MarketID", MarketID);
                    command.Parameters.AddWithValue("@ReceiptID", imageName);
                    command.Parameters.AddWithValue("@ReceiptStatus", status);
                    command.Parameters.AddWithValue("@UploadTime", UploadTime);
                    try
                    {
                        command.ExecuteNonQuery();
                        _logger.Info($"Succesful Updating family uploads, familyID: {selectedFamilyID}, MarketID: {MarketID}, imageName: {imageName} status: {status}, upload time: {UploadTime}");
                    }
                    catch (SqlException sqlException)
                    {
                        _logger.Error($"Error while Updating family uploads, familyID: {selectedFamilyID}, MarketID: {MarketID}, imageName: {imageName} status: {status}, upload time: {UploadTime}", sqlException);
                        throw sqlException;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public void insertReceiptData(string familyID, string receiptID, string productID, string productDescription, string productQuantity, string productPrice, double yCoordinate, bool validProduct)
        {
            _logger.Debug($"Inserting receipt data, familyID: {familyID}, receiptID: {receiptID}, productID: {productID}");
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
                    command.CommandText = "INSERT into ReceiptData(ReceiptID, ProductID, Description, Quantity, Price, YCoordinate, validProduct) VALUES(@ReceiptID, @ProductID, @Description, @Quantity, @Price, @YCoordinate, @validProduct)";
                    command.Parameters.AddWithValue("@ReceiptID", receiptID);
                    command.Parameters.AddWithValue("@ProductID", productID);
                    command.Parameters.AddWithValue("@Description", productDescription);
                    command.Parameters.AddWithValue("@Quantity", productQuantity);
                    command.Parameters.AddWithValue("@Price", productPrice);
                    command.Parameters.AddWithValue("@YCoordinate", yCoordinate.ToString());
                    command.Parameters.AddWithValue("@validProduct", validProduct);
                    try
                    {
                        command.ExecuteNonQuery();
                        _logger.Info($"Succesful Inserting receipt data, familyID: {familyID}, receiptID: {receiptID}, productID: {productID}");
                    }
                    catch (SqlException sqlException)
                    {
                        _logger.Debug($"Error while Inserting receipt data, familyID: {familyID}, receiptID: {receiptID}, productID: {productID}", sqlException);
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
            _logger.Debug($"Deleting receipt data, receiptID: {receiptID}");
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
                        _logger.Info($"Succesful Deleting receipt data, receiptID: {receiptID}");
                    }
                    catch (SqlException sqlException)
                    {
                        _logger.Debug($"Error while Deleting receipt data, receiptID: {receiptID}", sqlException);
                        //throw sqlException;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public string getSaltValue(string username)
        {
            _logger.Debug($"Getting salt value, username: {username}");
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
                    command.CommandText = "SELECT Salt FROM AuthorizedUsers WHERE UserName = @UserName";
                    command.Parameters.AddWithValue("@UserName", username);
                    try
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            String salt = "";
                            while (reader.Read())
                            {
                                salt += reader[0];
                            }
                            _logger.Info($"Succesful Getting salt value, username: {username}");
                            return salt;
                        }
                    }
                    catch (SqlException sqlException)
                    {
                        _logger.Error($"Error while Getting salt value, username: {username}", sqlException);
                        throw sqlException;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }
        public bool checkedUsernameAndPassword(string username, string password)
        {
            _logger.Debug($"Checking username and password, username: {username}");
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
                    command.CommandText = "SELECT * FROM AuthorizedUsers WHERE UserName = @UserName AND HashedPassword = @Password";
                    command.Parameters.AddWithValue("@UserName", username);
                    command.Parameters.AddWithValue("@Password", password);
                    try
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                _logger.Info($"Valid Username: {username} and password");
                                return true;
                            }

                            else
                            {
                                _logger.Debug($"Invalid Username: {username} and password");
                                return false;
                            }
                        }
                    }
                    catch (SqlException sqlException)
                    {
                        _logger.Error($"Error while Checking username and password, username: {username}", sqlException);
                        throw sqlException;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public string getGroupID(string username)
        {
            _logger.Debug($"Getting groupId, username: {username}");
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
                    command.CommandText = "SELECT GroupID FROM AuthorizedUsers WHERE UserName = @UserName";
                    command.Parameters.AddWithValue("@UserName", username);
                    try
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            String groupID = "";
                            while (reader.Read())
                            {
                                groupID += reader[0];
                            }
                            _logger.Info($"Succesful Getting groupId {groupID} for username: {username}");
                            return groupID;
                        }
                    }
                    catch (SqlException sqlException)
                    {
                        _logger.Error($"Error while getting groupid, username: {username}", sqlException);
                        throw sqlException;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public List<string> getFamilies()
        {
            _logger.Debug($"Getting families");
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
                    command.CommandText = "SELECT DISTINCT UserName FROM AuthorizedUsers";
                    try
                    {
                        List<string> Families = new List<string>();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Families.Add(reader[0].ToString());
                            }
                            _logger.Info($"Succesful Getting families");
                            return Families;
                        }
                    }
                    catch (SqlException sqlException)
                    {
                        _logger.Error($"Error while getting families", sqlException);
                        throw sqlException;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public void AddNewFamilyUser(string username, string salt, string hashedPassword, int groupID)
        {
            _logger.Debug($"Adding new family, username: {username}, groupID: {groupID}");
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
                    command.CommandText = "INSERT into AuthorizedUsers(UserName, Salt, HashedPassword, GroupID) VALUES(@UserName, @Salt, @HashedPassword, @GroupID)";
                    command.Parameters.AddWithValue("@UserName", username);
                    command.Parameters.AddWithValue("@Salt", salt);
                    command.Parameters.AddWithValue("@HashedPassword", hashedPassword);
                    command.Parameters.AddWithValue("@GroupID", groupID);
                    try
                    {
                        command.ExecuteNonQuery();
                        _logger.Info($"Succesful Adding new family: {username}, groupID: {groupID}");
                    }
                    catch (SqlException sqlException)
                    {
                        _logger.Error($"Error while adding new family: {username}, groupID: {groupID}", sqlException);
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
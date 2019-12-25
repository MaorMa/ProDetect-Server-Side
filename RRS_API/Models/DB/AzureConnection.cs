using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using log4net;
using System.Reflection;
using RRS_API.Models.Objects;

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
                _logger.Error($"Error while connecting to SQL Server", e);
                throw new Exception("35", e);
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
                    catch (Exception sqlException)
                    {
                        _logger.Error($"Error while Updating receipt status, familyID: {FamilyID}, ReceiptID: {ReceiptID}", sqlException);
                        throw new Exception("118", sqlException);
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
                    catch (Exception sqlException)
                    {
                        _logger.Error($"Error while Updating family uploads, familyID: {selectedFamilyID}, MarketID: {MarketID}, imageName: {imageName} status: {status}, upload time: {UploadTime}", sqlException);
                        throw new Exception("158", sqlException);
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
                    catch (Exception sqlException)
                    {
                        _logger.Debug($"Error while Inserting receipt data, familyID: {familyID}, receiptID: {receiptID}, productID: {productID}", sqlException);
                        throw new Exception("197", sqlException);
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
                    catch (Exception sqlException)
                    {
                        _logger.Debug($"Error while Deleting receipt data, receiptID: {receiptID}", sqlException);
                        throw new Exception("234", sqlException);
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
                    catch (Exception sqlException)
                    {
                        _logger.Error($"Error while Getting salt value, username: {username}", sqlException);
                        throw new Exception("275", sqlException);
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
                    catch (Exception sqlException)
                    {
                        _logger.Error($"Error while Checking username and password, username: {username}", sqlException);
                        throw new Exception("320", sqlException);
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
                    catch (Exception sqlException)
                    {
                        _logger.Error($"Error while getting groupid, username: {username}", sqlException);
                        throw new Exception("361", sqlException);
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
                    catch (Exception exception)
                    {
                        _logger.Error($"Error while getting families", exception);
                        throw new Exception("401", exception);
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
                    catch (Exception exception)
                    {
                        _logger.Error($"Error while adding new family: {username}, groupID: {groupID}", exception);
                        throw new Exception("437", exception);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }


        public Dictionary<string, string> getSimiliarProductNames(string productName)
        {
            _logger.Debug($"Getting SimiliarProductNames");
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
                    command.CommandText = "SELECT SID, Description FROM Nutrients WHERE Description LIKE N'%" + productName + " %' OR Description=N'" + productName + "'";
                    try
                    {
                        Dictionary<string, string> similiarProductNames = new Dictionary<string, string>();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //var i0 = reader[0];
                                //var i1 = reader[1];
                                if (!similiarProductNames.ContainsKey(reader[0].ToString()))
                                    similiarProductNames.Add(reader[0].ToString(), reader[1].ToString());
                            }
                            _logger.Info($"Succesful Getting SimiliarProductNames");
                            return similiarProductNames;
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.Error($"Error while Getting SimiliarProductNames", exception);
                        throw new Exception("481", exception);

                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        /*
        * Insert top 5 similar products to OptionalProducts table
        */
        //public void insertOptionalProducts(string productId, string SID, string OptionalName)
        public void insertOptionalProducts(string productId, List<ResearchProduct> optionalProducts)
        {
            _logger.Debug($"Inserting optional products for ProductId: {productId}");
            if (productExistsInOptionalProducts(productId))
                return;
            foreach (ResearchProduct rp in optionalProducts)
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

                        command.CommandText = "INSERT into OptionalProducts(ProductID, SID, OptionalName) VALUES(@ProductID, @SID, @OptionalName)";
                        command.Parameters.AddWithValue("@ProductID", productId);
                        command.Parameters.AddWithValue("@SID", rp.SID);
                        command.Parameters.AddWithValue("@OptionalName", rp.Name);
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception sqlException)
                        {
                            connection.Close();
                            throw new Exception("524",sqlException);
                        }
                    }
                    connection.Close();
                }
            }
        }

        /*
         * Checks if product already exists in OptionalProducts table (was already selected in the past)
         */
        private bool productExistsInOptionalProducts(string productId)
        {
            _logger.Debug($"Checking if productId: {productId} exists in OptionalProducts");
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
                    command.CommandText = "SELECT * FROM OptionalProducts WHERE ProductID = @productId";
                    command.Parameters.AddWithValue("@productId", productId);
                    try
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                _logger.Info($"productId: {productId} exists");
                                return true;
                            }

                            else
                            {
                                _logger.Debug($"productId: {productId} does NOT exists");
                                return false;
                            }
                        }
                    }
                    catch (Exception sqlException)
                    {
                        _logger.Error($"Error while Checking if productId: {productId} exists", sqlException);
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
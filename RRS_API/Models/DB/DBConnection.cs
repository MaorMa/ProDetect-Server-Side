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

    public class DBConnection
    {
        private SqlConnectionStringBuilder builder;
        private static DBConnection INSTANCE;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /*
         * create connection to database
         * implements singelton design pattern
         */
        private DBConnection()
        {
            try
            {
                _logger.Debug("Trying to create connection to SQL Server");
                builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["DBConnection"].ToString());
                _logger.Info("Succesful connection to SQL Server");
            }
            catch (Exception e)
            {
                _logger.Error($"Error while connecting to SQL Server", e);
                throw new Exception("35", e);
            }
        }

        public static DBConnection GetInstance()
        {
            if(INSTANCE == null)
            {
                INSTANCE = new DBConnection();
            }
            return INSTANCE;
        }

        /*
         * generic select query 
         * return List contains query result
         * seperated by comma 
         */
        public List<string> SelectQuery(string query)
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
        public void UpdateStatus(string FamilyID, string ReceiptID, string status)
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
        public void UpdateFamilyUploads(string selectedFamilyID, string MarketID, string imageName, int status, string UploadTime)
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

        public void InsertReceiptData(string familyID, string receiptID, string productID, string productDescription, string productQuantity, string productDescriptionQuantity, string productPrice, double yCoordinate, bool validProduct)
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
                    command.CommandText = "INSERT into ReceiptData(ReceiptID, ProductID, Description, Quantity, DescriptionQuantity, Price, YCoordinate, ValidProduct) VALUES(@ReceiptID, @ProductID, @Description, @Quantity, @DescriptionQuantity, @Price, @YCoordinate, @ValidProduct)";
                    command.Parameters.AddWithValue("@ReceiptID", receiptID);
                    command.Parameters.AddWithValue("@ProductID", productID);
                    command.Parameters.AddWithValue("@Description", productDescription);
                    command.Parameters.AddWithValue("@Quantity", productQuantity);
                    command.Parameters.AddWithValue("@DescriptionQuantity", productDescriptionQuantity);
                    command.Parameters.AddWithValue("@Price", productPrice);
                    command.Parameters.AddWithValue("@YCoordinate", yCoordinate.ToString());
                    command.Parameters.AddWithValue("@ValidProduct", validProduct);
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
        public void DeleteReceiptData(string receiptID)
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

        /*
         * delete products of given receipt
         */
        public void DeleteFamilyReceipt(string receiptID)
        {
            _logger.Debug($"Deleting receipt from family, receiptID: {receiptID}");
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
                    command.CommandText = "DELETE FROM FamilyUploads WHERE ReceiptID = @ReceiptID";
                    command.Parameters.AddWithValue("@ReceiptID", receiptID);
                    try
                    {
                        command.ExecuteNonQuery();
                        _logger.Info($"Succesful Deleting receipt from family, receiptID: {receiptID}");
                    }
                    catch (Exception e)
                    {
                        _logger.Debug($"Error while Deleting receipt from family, receiptID: {receiptID}", e);
                        throw new Exception("234", e);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        /*
         * delete all optional Data
         */
        public void DeleteOptionalData(string marketId, string productID)
        {
            _logger.Debug($"Deleting optional data, marketID:{marketId} productID: {productID}");
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
                    command.CommandText = "DELETE FROM OptionalProducts WHERE MarketID = @MarketID AND ProductID = @ProductID";
                    command.Parameters.AddWithValue("@MarketID", marketId);
                    command.Parameters.AddWithValue("@ProductID", productID);
                    try
                    {
                        command.ExecuteNonQuery();
                        _logger.Info($"Succesful Deleting optional data, marketID:{marketId} ProductID: {productID}");
                    }
                    catch (Exception sqlException)
                    {
                        _logger.Debug($"Error while Deleting optional data, marketID:{marketId} ProductID: {productID}", sqlException);
                        throw new Exception("267", sqlException);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public string GetSaltValue(string username)
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

        public bool CheckUsernameAndPassword(string username, string password)
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

        public string GetGroupID(string username)
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

        public List<string> GetFamilies()
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
                    command.CommandText = "SELECT UserName FROM AuthorizedUsers";
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

        public List<string> GetFamiliesWithApprovedData()
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
                    command.CommandText = "SELECT DISTINCT FamilyID FROM FamilyUploads WHERE ReceiptStatus=1";
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


        public Dictionary<string, string> GetSimiliarProductNames(string productName)
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
                    command.CommandText = "SELECT SID, Description FROM Nutrients WHERE Description LIKE N'%' + @Description + ' %' OR Description LIKE N'%' + @Description + '%' OR Description=N' + @Description + '";
                    command.Parameters.AddWithValue("@Description", productName);
                    try
                    {
                        Dictionary<string, string> similiarProductNames = new Dictionary<string, string>();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {

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
        public void InsertOptionalProducts(string marketId, string productId, List<ResearchProduct> optionalProducts)
        {
            _logger.Debug($"Inserting optional products for ProductId: {productId}");
            if (ProductExistsInOptionalProducts(marketId, productId))
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

                        command.CommandText = "INSERT into OptionalProducts(MarketID, ProductID, SID, OptionalName, Similarity) VALUES(@MarketID, @ProductID, @SID, @OptionalName, @Similarity)";
                        command.Parameters.AddWithValue("@MarketID", marketId);
                        command.Parameters.AddWithValue("@ProductID", productId);
                        command.Parameters.AddWithValue("@SID", rp.sID);
                        command.Parameters.AddWithValue("@OptionalName", rp.name);
                        command.Parameters.AddWithValue("@Similarity", rp.similarity);
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception sqlException)
                        {
                            connection.Close();
                            throw new Exception("526",sqlException);
                        }
                    }
                    connection.Close();
                }
            }
        }

        public void InsertOptionalProduct(string marketId, string productId, ResearchProduct optionalProduct)
        {
            _logger.Debug($"Inserting optional products for ProductId: {productId}");
            if (ProductExistsInOptionalProducts(marketId, productId))
                return;
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

                    command.CommandText = "INSERT into OptionalProducts(MarketID, ProductID, SID, OptionalName, Similarity) VALUES(@MarketID, @ProductID, @SID, @OptionalName, @Similarity)";
                    command.Parameters.AddWithValue("@MarketID", marketId);
                    command.Parameters.AddWithValue("@ProductID", productId);
                    command.Parameters.AddWithValue("@SID", optionalProduct.sID);
                    command.Parameters.AddWithValue("@OptionalName", optionalProduct.name);
                    command.Parameters.AddWithValue("@Similarity", optionalProduct.similarity);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception sqlException)
                    {
                        connection.Close();
                        throw new Exception("526", sqlException);
                    }
                }
                connection.Close();
            }
        }


        //delete old nutrients data
        public void DeleteNutrientsData()
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
                    command.CommandText = "DELETE FROM Test";
                    try
                    {
                        command.ExecuteNonQuery();
                        _logger.Info($"Succesful Deleting Nutrients");
                    }
                    catch (Exception sqlException)
                    {
                        _logger.Debug($"Error while Deleting Nutrients", sqlException);
                        throw new Exception("671", sqlException);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        //insert new nutrient
        public void InsertNewNutrient(string foodCode,string foodName, string nutrients)
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
                    command.CommandText = "INSERT into Test(SID, Description, nut203, nut204,nut205,nut208,nut221,nut255,nut291,nut301,nut303,nut304,nut305,nut306,nut307,nut309,nut312,nut318,nut320,nut321,nut323,nut401,nut404,nut405,nut406,nut415,nut417,nut418,nut601,nut606,nut607,nut608,nut609,nut610,nut611,nut612,nut613,nut614,nut617,nut618,nut619,nut620,nut621,nut622,nut623,nut628,nut625,nut630,nut631,nut645,nut646,nut324,nut269,nut605) VALUES(@SID, @Description,@nut203, @nut204,@nut205,@nut208,@nut221,@nut255,@nut291,@nut301,@nut303,@nut304,@nut305,@nut306,@nut307,@nut309,@nut312,@nut318,@nut320,@nut321,@nut323,@nut401,@nut404,@nut405,@nut406,@nut415,@nut417,@nut418,@nut601,@nut606,@nut607,@nut608,@nut609,@nut610,@nut611,@nut612,@nut613,@nut614,@nut617,@nut618,@nut619,@nut620,@nut621,@nut622,@nut623,@nut628,@nut625,@nut630,@nut631,@nut645,@nut646,@nut324,@nut269,@nut605)";
                    string[] splittedLine = nutrients.Split(',');
                    command.Parameters.AddWithValue("@SID", foodCode);
                    command.Parameters.AddWithValue("@Description", foodName);
                    command.Parameters.AddWithValue("@nut203", splittedLine[0]);
                    command.Parameters.AddWithValue("@nut204", splittedLine[1]);
                    command.Parameters.AddWithValue("@nut205", splittedLine[2]);
                    command.Parameters.AddWithValue("@nut208", splittedLine[3]);
                    command.Parameters.AddWithValue("@nut221", splittedLine[4]);
                    command.Parameters.AddWithValue("@nut255", splittedLine[5]);
                    command.Parameters.AddWithValue("@nut291", splittedLine[6]);
                    command.Parameters.AddWithValue("@nut301", splittedLine[7]);
                    command.Parameters.AddWithValue("@nut303", splittedLine[8]);
                    command.Parameters.AddWithValue("@nut304", splittedLine[9]);
                    command.Parameters.AddWithValue("@nut305", splittedLine[10]);
                    command.Parameters.AddWithValue("@nut306", splittedLine[11]);
                    command.Parameters.AddWithValue("@nut307", splittedLine[12]);
                    command.Parameters.AddWithValue("@nut309", splittedLine[13]);
                    command.Parameters.AddWithValue("@nut312", splittedLine[14]);
                    command.Parameters.AddWithValue("@nut318", splittedLine[15]);
                    command.Parameters.AddWithValue("@nut320", splittedLine[16]);
                    command.Parameters.AddWithValue("@nut321", splittedLine[17]);
                    command.Parameters.AddWithValue("@nut323", splittedLine[18]);
                    command.Parameters.AddWithValue("@nut401", splittedLine[19]);
                    command.Parameters.AddWithValue("@nut404", splittedLine[20]);
                    command.Parameters.AddWithValue("@nut405", splittedLine[21]);
                    command.Parameters.AddWithValue("@nut406", splittedLine[22]);
                    command.Parameters.AddWithValue("@nut415", splittedLine[23]);
                    command.Parameters.AddWithValue("@nut417", splittedLine[24]);
                    command.Parameters.AddWithValue("@nut418", splittedLine[25]);
                    command.Parameters.AddWithValue("@nut601", splittedLine[26]);
                    command.Parameters.AddWithValue("@nut606", splittedLine[27]);
                    command.Parameters.AddWithValue("@nut607", splittedLine[28]);
                    command.Parameters.AddWithValue("@nut608", splittedLine[29]);
                    command.Parameters.AddWithValue("@nut609", splittedLine[30]);
                    command.Parameters.AddWithValue("@nut610", splittedLine[31]);
                    command.Parameters.AddWithValue("@nut611", splittedLine[32]);
                    command.Parameters.AddWithValue("@nut612", splittedLine[33]);
                    command.Parameters.AddWithValue("@nut613", splittedLine[34]);
                    command.Parameters.AddWithValue("@nut614", splittedLine[35]);
                    command.Parameters.AddWithValue("@nut617", splittedLine[36]);
                    command.Parameters.AddWithValue("@nut618", splittedLine[37]);
                    command.Parameters.AddWithValue("@nut619", splittedLine[38]);
                    command.Parameters.AddWithValue("@nut620", splittedLine[39]);
                    command.Parameters.AddWithValue("@nut621", splittedLine[40]);
                    command.Parameters.AddWithValue("@nut622", splittedLine[41]);
                    command.Parameters.AddWithValue("@nut623", splittedLine[42]);
                    command.Parameters.AddWithValue("@nut628", splittedLine[43]);
                    command.Parameters.AddWithValue("@nut625", splittedLine[44]);
                    command.Parameters.AddWithValue("@nut630", splittedLine[45]);
                    command.Parameters.AddWithValue("@nut631", splittedLine[46]);
                    command.Parameters.AddWithValue("@nut645", splittedLine[47]);
                    command.Parameters.AddWithValue("@nut646", splittedLine[48]);
                    command.Parameters.AddWithValue("@nut324", splittedLine[49]);
                    command.Parameters.AddWithValue("@nut269", splittedLine[50]);
                    command.Parameters.AddWithValue("@nut605", splittedLine[51]);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception exception)
                    {
                        throw new Exception("724", exception);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        /*
         * Checks if product already exists in OptionalProducts table (was already selected in the past)
         */
        private bool ProductExistsInOptionalProducts(string marketId, string productId)
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
                    command.CommandText = "SELECT * FROM OptionalProducts WHERE MarketID = @marketId AND ProductID = @productId";
                    command.Parameters.AddWithValue("@marketId", marketId);
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
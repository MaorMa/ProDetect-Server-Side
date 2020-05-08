using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using log4net;
using System.Reflection;
using System.IdentityModel.Tokens.Jwt;
using RRS_API.Models.Objects;

namespace RRS_API.Models.Mangagers
{
    /// <summary>
    /// This class responsilbe to manage operations related to user.
    /// This class can add new user, check if user is global admin, check if local admin, check login credentials.
    /// </summary>
    public class UsersMngr
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        PasswordMngr passwordMngr = new PasswordMngr();
        private DBConnection DBConnection = DBConnection.GetInstance();
        TokenMngr tokenMngr = new TokenMngr();

        /// <summary>
        /// This method add new user to the systme.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void AddNewFamilyUser(string username, string password)
        {
            try
            {
                _logger.Debug($"Adding new user: {username}");
                var salt_bytes = passwordMngr.GenerateSalt(); //new salt byetes
                string salt = Convert.ToBase64String(salt_bytes);
                salt_bytes = Encoding.UTF8.GetBytes(salt);
                var password_bytes = Encoding.UTF8.GetBytes(password);
                string hashedPass = passwordMngr.Sha256Encription(password_bytes, salt_bytes); //generate hashed password
                DBConnection.AddNewFamilyUser(username, salt, hashedPass, 0);
            }catch(Exception e)
            {
                _logger.Error($"User already exists", e);
                throw new Exception("User already exists",e);

            }
        }

        /// <summary>
        /// This method check if user is global admin by user's token.
        /// </summary>
        /// <param name="token">user's token</param>
        /// <returns>True - if global admin, otherwise False.</returns>
        public bool IsGlobalAdmin(string token)
        {
            bool isTokenValid = tokenMngr.IsTokenValid(token);
            bool admin = isAdmin(token);
            bool experimentUser = isExperimentUser(token);
            return isTokenValid && admin && !experimentUser;
        }

        /// <summary>
        /// This method check if user is local admin by user's token.
        /// </summary>
        /// <param name="token">user's token</param>
        /// <returns>True - if local admin, otherwise False. </returns>
        public bool IsLocalAdmin(string token)
        {
            bool isTokenValid = tokenMngr.IsTokenValid(token);
            bool admin = isAdmin(token);
            bool experimentUser = isExperimentUser(token);
            return isTokenValid && admin && experimentUser;
        }

        /// <summary>
        /// This method check if user token valid. (using Token manager)
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True - token valid, otherwise - False. </returns>
        public bool IsUserTokenValid(string token)
        {
            return tokenMngr.IsTokenValid(token);
        }

        /// <summary>
        /// This method check login credentials.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>if credinatials are valid, return token. </returns>
        public string CheckLoginCredentials(string username, string password)
        {
            var salt_value = DBConnection.GetSaltValue(username);
            var salt_bytes = Encoding.UTF8.GetBytes(salt_value);
            var password_bytes = Encoding.UTF8.GetBytes(password);
            string hashedPass = passwordMngr.Sha256Encription(password_bytes, salt_bytes);
            
            //if valid credentials generate token
            if (DBConnection.CheckUsernameAndPassword(username, hashedPass))
            {
                return tokenMngr.GenerateToken(username);
            }

            //forbidden
            else
            {
                _logger.Debug($"Token for username {username} is Forbidden");
                throw new Exception("Forbidden");
            }
        }


        /// <summary>
        /// This mehod check if user is admin (local or global)
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True - if admin, otherwise False. </returns>
        private bool isAdmin(string token)
        {
            _logger.Debug($"Checking token for admin privileges");
            var jwtToken = new JwtSecurityToken(token);
            object username = "";
            jwtToken.Payload.TryGetValue("unique_name", out username);
            string groupID = DBConnection.GetGroupID(username.ToString());
            return groupID.Equals("True");
        }

        private bool isExperimentUser(string token)
        {
            var jwtToken = new JwtSecurityToken(token);
            object username = "";
            jwtToken.Payload.TryGetValue("unique_name", out username);
            List<string> records = DBConnection.SelectQuery("SELECT * FROM Experiments WHERE FamilyID='" + username + "'");
            bool isExperimentUser = records.Count > 0;
            return isExperimentUser;
        }

        public string getUsernameByToken(string token)
        {
            return tokenMngr.getUsernameByToken(token);
        }
    }
}
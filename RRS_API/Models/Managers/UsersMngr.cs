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
    public class UsersMngr
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        PasswordMngr passwordMngr = new PasswordMngr();
        private DBConnection DBConnection = DBConnection.GetInstance();
        TokenMngr tokenMngr = new TokenMngr();


        public void AddNewFamilyUser(string username, string password)
        {
            _logger.Debug($"Adding new user: {username}");
            var salt_bytes = passwordMngr.GenerateSalt(); //new salt byetes
            string salt = Convert.ToBase64String(salt_bytes);
            salt_bytes = Encoding.UTF8.GetBytes(salt);
            var password_bytes = Encoding.UTF8.GetBytes(password);
            string hashedPass = passwordMngr.Sha256Encription(password_bytes, salt_bytes); //generate hashed password
            DBConnection.AddNewFamilyUser(username, salt, hashedPass, 0);
        }

        public bool IsGlobalAdmin(string token)
        {
            bool isTokenValid = tokenMngr.IsTokenValid(token);
            bool admin = isAdmin(token);
            bool experimentUser = isExperimentUser(token);
            return isTokenValid && admin && !experimentUser;
        }

        public bool IsLocalAdmin(string token)
        {
            bool isTokenValid = tokenMngr.IsTokenValid(token);
            bool admin = isAdmin(token);
            bool experimentUser = isExperimentUser(token);
            return isTokenValid && admin && experimentUser;
        }

        public bool IsUserTokenValid(string token)
        {
            return tokenMngr.IsTokenValid(token);
        }

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
    }
}
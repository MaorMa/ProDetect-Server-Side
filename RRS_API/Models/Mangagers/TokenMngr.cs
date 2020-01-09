using RRS_API.Models.JWT;
using RRS_API.Models.Mangagers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using log4net;
using System.Reflection;

namespace RRS_API.Models.Objects
{
    public class TokenMngr : AMngr
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IAuthContainerModel model = new JWTContainer();
        private IAuthService authService;

        #region Public Methods
        /*
         * generate a toekn for authorized user
         */
        public string generateToken(string username, string password, string hashedPass)
        {
            _logger.Debug($"Generating token for username: {username}");
            if (DBConnection.getInstance().checkedUsernameAndPassword(username, hashedPass))
            {
                model = GetJWTContainerModel(username, password);
                authService = new JWTService(model.secretKey);
                _logger.Info($"Succesful Generating token for username: {username}");
                return authService.generateToken(model);
            }
            else
            {
                _logger.Debug($"Token for username {username} is Forbidden");
                throw new Exception("Forbidden");
            }
        }

        public bool isAdmin(string token)
        {
            _logger.Debug($"Checking token for admin privileges");
            var jwtToken = new JwtSecurityToken(token);
            object username = "";
            jwtToken.Payload.TryGetValue("unique_name", out username);
            string groupID =  AzureConnection.getGroupID(username.ToString());
            return groupID.Equals("True");
        }

        /*
         * if token is valid - return true
         * else return false
         */
        public bool isTokenValid(string token)
        {
            _logger.Debug($"Checking if token is valid");
            authService = new JWTService(model.secretKey);
            if (authService.isTokenValid(token))
            {
                _logger.Info($"Token is valid");
                return true;
            }
            else
            {
                _logger.Info($"Token is invalid");
                return false;
            }
        }

        public JWTContainer GetJWTContainerModel(string username, string psw)
        {
            return new JWTContainer()
            {
                claims = new Claim[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Email, psw)
                }
            };
        }

        #endregion
    }
}
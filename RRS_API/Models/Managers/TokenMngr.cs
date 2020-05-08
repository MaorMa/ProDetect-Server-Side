using RRS_API.Models.JWT;
using RRS_API.Models.Mangagers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using log4net;
using System.Reflection;

namespace RRS_API.Models.Objects
{
    /// <summary>
    /// This class responsilbe to manage tokens.
    /// <remarks>
    /// This class can generate token, check if token valid, extract user name from token.
    /// </remarks>
    /// </summary>
    public class TokenMngr
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IAuthContainerModel model = new JWTContainer();
        private IAuthService authService;

        #region Public Methods
        /// <summary>
        /// This method generate token for authorized user.
        /// </summary>
        /// <param name="username"> authorized user name</param>
        /// <returns>Token</returns>
        public string GenerateToken(string username)
        {
            //_logger.Debug($"Generating token for username: {username}");
            model = GetJWTContainerModel(username);
            authService = new JWTService(model.secretKey);
            //_logger.Info($"Succesful Generating token for username: {username}");
            return authService.GenerateToken(model);
        }

         /// <summary>
         /// This method check if given token is valid.
         /// </summary>
         /// <param name="token"></param>
         /// <returns>True - it token is valid, otherwise False</returns>
        public bool IsTokenValid(string token)
        {
            //_logger.Debug($"Checking if token is valid");
            authService = new JWTService(model.secretKey);
            if (authService.IsTokenValid(token))
            {
                //_logger.Info($"Token is valid");
                return true;
            }
            else
            {
                //_logger.Info($"Token is invalid");
                return false;
            }
        }

        /// <summary>
        /// This method extract username from given token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>user name</returns>
        public string getUsernameByToken(string token)
        {
            var jwtToken = new JwtSecurityToken(token);
            object username = "";
            jwtToken.Payload.TryGetValue("unique_name", out username);
            return username.ToString();
        }

        private JWTContainer GetJWTContainerModel(string username)
        {
            return new JWTContainer()
            {
                claims = new Claim[]
                {
                    new Claim(ClaimTypes.Name, username),
                }
            };
        }

        #endregion
    }
}
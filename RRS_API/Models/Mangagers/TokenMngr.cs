using RRS_API.Models.JWT;
using RRS_API.Models.Mangagers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RRS_API.Models.Objects
{
    public class TokenMngr : AMngr
    {
        private IAuthContainerModel model = new JWTContainer();
        private IAuthService authService;

        #region Public Methods
        /*
         * generate a toekn for authorized user
         */
        public string generateToken(string username, string password, string hashedPass)
        {
            if (AzureConnection.getInstance().checkedUsernameAndPassword(username, hashedPass))
            {
                model = GetJWTContainerModel(username, password);
                authService = new JWTService(model.secretKey);
                return authService.generateToken(model);
            }
            else
            {
                throw new Exception("Forbidden");
            }
        }

        public bool isAdmin(string token)
        {
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
            authService = new JWTService(model.secretKey);
            if (authService.isTokenValid(token))
            {
                return true;
            }
            else
            {
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
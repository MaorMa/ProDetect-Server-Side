using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace RRS_API.Models.JWT
{
    public class JWTContainer : IAuthContainerModel
    {
        public Claim[] claims
        {
            get; set;
        }

        public int exprieTime
        {
            get; set;
        } = 1440;

        public string secretKey
        {
            get; set;
        } = "ZGZkZ2ZnZGZnZjU2NDVkcmdlcmd3MzQzNHdm";

        public string securityAlgorithm
        {
            get; set;
        } = SecurityAlgorithms.HmacSha256Signature;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RRS_API.Models.JWT
{
    public interface IAuthContainerModel
    {
        string secretKey { get; set; }
        string securityAlgorithm { get; set; }
        int expireTime { get; set; }
        Claim[] claims { get; set; }
    }
}

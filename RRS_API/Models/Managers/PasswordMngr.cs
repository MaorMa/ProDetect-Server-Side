using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using log4net;
using System.Reflection;

namespace RRS_API.Models.Mangagers
{
    /*
     * This class responsible for hashing 
     */ 
    public class PasswordMngr
    {
        private const int SaltSize = 32;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public byte[] GenerateSalt()
        {
            _logger.Debug("Generating salt");
            using (var rng = new RNGCryptoServiceProvider())
            {
                var randomNumber = new byte[SaltSize];

                rng.GetBytes(randomNumber);

                return randomNumber;

            }
        }

        public string Sha256Encription(byte[] password, byte[] salt)
        {
            using (var hmac = new HMACSHA256(salt))
            {
                return Convert.ToBase64String(hmac.ComputeHash(password));
            }
        }

    }
}
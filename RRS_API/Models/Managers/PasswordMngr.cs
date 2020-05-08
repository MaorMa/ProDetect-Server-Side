using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using log4net;
using System.Reflection;

namespace RRS_API.Models.Mangagers
{
    /// <summary>
    /// This class responsible for manage passwords.
    /// <remarks>
    /// This class can generate salt value, encrypt password by sha 256 algorithm.
    /// </remarks>
    /// </summary>
    public class PasswordMngr
    {
        private const int SaltSize = 32;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// This method generate random salt value.
        /// </summary>
        /// <returns>random salt value. </returns>
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

        /// <summary>
        /// This class encrypt password using given salt value.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns> encrypted password. </returns>
        public string Sha256Encription(byte[] password, byte[] salt)
        {
            using (var hmac = new HMACSHA256(salt))
            {
                return Convert.ToBase64String(hmac.ComputeHash(password));
            }
        }

    }
}
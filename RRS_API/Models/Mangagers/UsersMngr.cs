using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace RRS_API.Models.Mangagers
{
    public class UsersMngr
    {
        PasswordMngr passwordMnr = new PasswordMngr();

        public void AddNewFamilyUser(string username, string password)
        {
            var salt_bytes = passwordMnr.GenerateSalt(); //new salt byetes
            string salt = Convert.ToBase64String(salt_bytes);
            salt_bytes = Encoding.UTF8.GetBytes(salt);
            var password_bytes = Encoding.UTF8.GetBytes(password);
            string hashedPass = Convert.ToBase64String(passwordMnr.ComputeHMAC_SHA256(password_bytes, salt_bytes)); //generate hashed password
            AzureConnection.getInstance().AddNewFamilyUser(username, salt, hashedPass, 0);
        }
    }
}
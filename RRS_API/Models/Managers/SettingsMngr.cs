using RRS_API.Models.Mangagers;
using System;
using System.Collections.Generic;

namespace RRS_API.Models
{
    public class SettingsMngr
    {
        private DBConnection DBConnection = DBConnection.GetInstance();

        #region Public Methods
        public List<String> GetMarkets()
        {
            string query = "SELECT * FROM Markets";
            return DBConnection.SelectQuery(query);
        }

        public List<string> GetFamilies()
        {
            return DBConnection.GetFamilies();
        }
        #endregion
    }
}
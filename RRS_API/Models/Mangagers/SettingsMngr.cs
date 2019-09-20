using RRS_API.Models.Mangagers;
using System;
using System.Collections.Generic;

namespace RRS_API.Models
{
    public class SettingsMngr : AMngr
    {
        #region Public Methods
        public List<String> getMarkets()
        {
            string query = "SELECT * FROM Markets";
            return AzureConnection.SelectQuery(query);
        }

        public List<string> getFamilies()
        {
            return AzureConnection.getFamilies();
        }
        #endregion
    }
}
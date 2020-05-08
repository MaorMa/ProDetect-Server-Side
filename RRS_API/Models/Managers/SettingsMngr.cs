using RRS_API.Models.Mangagers;
using System;
using System.Collections.Generic;

namespace RRS_API.Models
{
    /// <summary>
    /// This class responsible to return basic settings.
    /// </summary>
    public class SettingsMngr
    {
        private DBConnection DBConnection = DBConnection.GetInstance();

        #region Public Methods
        /// <summary>
        /// This method return all existing markets in db.
        /// </summary>
        /// <returns>List of Market's names</returns>
        public List<String> GetMarkets()
        {
            string query = "SELECT * FROM Markets";
            return DBConnection.SelectQuery(query);
        }

        /// <summary>
        /// This method return all existing families in db.
        /// </summary>
        /// <returns>List of famile's names</returns>
        public List<string> GetFamilies()
        {
            return DBConnection.GetFamilies();
        }
        #endregion
    }
}
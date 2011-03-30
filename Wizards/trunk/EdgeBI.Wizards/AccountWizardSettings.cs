using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;

namespace EdgeBI.Wizards
{
    public class AccountWizardSettings : Dictionary<string, object>
    {

        public AccountWizardSettings(int applicationID)
        {
            using (DataManager.Current.OpenConnection())
            {
                using (SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT Setting_Name,Value 
                                                                FROM Account_Wizard_Settings
                                                                WHERE Application=0 OR Application=@ApplicationID:Int"))
                {
                    sqlCommand.Parameters["@ApplicationID"].Value = applicationID;
                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            if (!this.ContainsKey(sqlDataReader["Setting_Name"].ToString()))
                            {
                                this.Add(sqlDataReader["Setting_Name"].ToString(), sqlDataReader["Value"]);
                            }
                            else
                                throw new Exception(string.Format("AccountWizardSettings: Contains Duplicate keys ({0} ", sqlDataReader["Setting_Name"]));

                        }

                    }

                }
            }

        }
        public List<string> GetLikeSettings(string likeSettings)
        {
            List<string> likeSettingsList = new List<string>();
            foreach (string str in this.Keys)
            {
                if (str.Contains(likeSettings))
                {
                    likeSettingsList.Add(str);                    
                }
            }
            return likeSettingsList;

        }

        public string Get(string key)
        {

            try
            {
                return this[key].ToString();
            }
            catch (Exception ex)
            {
                
                throw new Exception(string.Format("the given application key ({0}) cannot be found",key));
            }
        }





    }

}

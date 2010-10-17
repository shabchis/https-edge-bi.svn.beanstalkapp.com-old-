using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.Configuration;
using System.Reflection;
using Easynet.Edge.Core.Configuration;


namespace EdgeBI.Wizards.AccountWizard.CubeCreation
{
    /// <summary>
    /// This object will to care active directory management
    /// You have to add to your "App.config" the parameters below:
    /// <add key="ActiveDirectoryPath" value="LDAP path"/>
    /// Example to LDAP path: "LDAP://79.125.11.216/CN=Users,dc=edge,dc=bi"
    /// <add key="ActiveDirectoryLoginUserName" value="Addministrator Login" />
    /// <add key="ActiveDirectoryLoginPassword" value="Addministrator Password" />
    /// </summary>
    public class CreatingUsers
    {
        /// <summary>
        /// The AddNew function will add new user acount to Active Directory
        /// </summary>
        /// <param name="strLogin"></param>
        /// <param name="strPwd"></param>
        /// <param name="strFullName"></param>
        /// <param name="AccountActive"></param>
        public void AddNewActiveDirectoryUser(string strLogin, string strPwd, string strFullName, bool AccountActive)
        {


            DirectoryEntry obDirEntry = null;
           // obDirEntry = new DirectoryEntry(ConfigurationSettings.AppSettings["ActiveDirectoryPath"].ToString(), ConfigurationSettings.AppSettings["ActiveDirectoryLoginUserName"].ToString(), ConfigurationSettings.AppSettings["ActiveDirectoryLoginPassword"].ToString());//("LDAP://79.125.11.216/CN=Users,dc=edge,dc=bi","biadmin","Narnia2@");
			obDirEntry = new DirectoryEntry("LDAP://79.125.11.216/CN=Users,dc=edge,dc=bi", "biadmin", "Narnia2@");
			bool userFound = false;
            try
            {
				
                if (obDirEntry.Children.Find("CN=" + strLogin, "user") != null)
                    userFound = true;
            }
            catch (Exception)
            {
                userFound = false; //user not in the system
              //  MessageBox.Show("New User error: " + ex.ToString());
            }


            if (!userFound) //If User Not Found In System
            {
                DirectoryEntry obUser = obDirEntry.Children.Add("CN=" + strLogin, "user");
				
                obUser.Properties["sAMAccountName"].Add(strLogin);
				
                obUser.CommitChanges();
				object[] oPassword = new object[] { strPwd };
				object ret = obUser.Invoke("SetPassword", strPwd);
			
                obUser.CommitChanges();
                //UF_DONT_EXPIRE_PASSWD 0x10000
				UserAccountControlFlags exp = (UserAccountControlFlags)obUser.Properties["userAccountControl"].Value;
				obUser.Properties["userAccountControl"].Value = exp | UserAccountControlFlags.UF_DONT_EXPIRE_PASSWD; //what is this?
                obUser.CommitChanges();
                if (AccountActive)
                {
                    //UF_ACCOUNTDISABLE 0x0002
					UserAccountControlFlags val = (UserAccountControlFlags)obUser.Properties["userAccountControl"].Value;
					obUser.Properties["userAccountControl"].Value = val & ~UserAccountControlFlags.UF_ACCOUNTDISABLE;  //what is this?
                    obUser.CommitChanges();
                }
				obUser.Close();
				obUser.Dispose();
            }
			obDirEntry.Dispose();
        }
     }

	[Flags]
	public enum UserAccountControlFlags
	{
		UF_DONT_EXPIRE_PASSWD	= 0x10000,
		UF_ACCOUNTDISABLE		= 0x00002
	}
}

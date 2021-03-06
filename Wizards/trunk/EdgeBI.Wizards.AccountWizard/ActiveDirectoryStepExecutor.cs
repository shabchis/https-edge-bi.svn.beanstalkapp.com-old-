﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Services;
using EdgeBI.Wizards.AccountWizard;
using System.DirectoryServices;

namespace EdgeBI.Wizards.AccountWizard
{
	class ActiveDirectoryStepExecutor : StepExecuter
	{
		protected override ServiceOutcome DoWork()
		{

			
			
			Log.Write("Getting collected data from suitable collector", LogMessageType.Information);

            Dictionary<string, object> collectedData = this.GetCollectedData();
            if (collectedData.ContainsKey(ApplicationIDKey))
                SetAccountWizardSettingsByApllicationID(Convert.ToInt32(collectedData[ApplicationIDKey]));
			this.ReportProgress(0.5f);
			
			Log.Write("Creating user in active directory", LogMessageType.Information);
			try
			{
                if ( !(bool)collectedData["AccountSettings.UseExistingRole"])
                {
                    if (!string.IsNullOrEmpty(collectedData["ActiveDirectory.Password"].ToString()))
                    {
                        AddNewActiveDirectoryUser(collectedData["ActiveDirectory.UserName"].ToString(), collectedData["ActiveDirectory.Password"].ToString(), collectedData["ActiveDirectory.FullName"].ToString(), true);
                        //  this.SaveExecutorData(collectedData); todo: check why i did this line look like i dont need it
                        Log.Write(string.Format("User {0} created in active directory", collectedData["ActiveDirectory.UserName"].ToString()), LogMessageType.Information);
                        UpdateOltpDataBase(collectedData);
                        this.ReportProgress((float)0.9);
                    } 
                }
                this.ReportProgress((float)1);
			}
			catch (Exception ex)
			{
				Log.Write("User Can't be add!", ex, LogMessageType.Error);
				throw new Exception("User Can't be add!", ex);


			}
			finally
			{
				this.ReportProgress((float)1);

			}
            
			
			return base.DoWork();
		}

        
		/// <summary>
		/// The AddNew function will add new user acount to Active Directory
		/// </summary>
		/// <param name="strLogin"></param>
		/// <param name="strPwd"></param>
		/// <param name="strFullName"></param>
		/// <param name="AccountActive"></param>
		public void AddNewActiveDirectoryUser(string strLogin, string strPwd, string strFullName, bool AccountActive)
		{
			string ldapPath=accountWizardSettings.Get("LDAP.Path");
            string ldapUserName = Encryptor.Decrypt(accountWizardSettings.Get( "LDAP.UserName"));
            string ldapPassword = Encryptor.Decrypt(accountWizardSettings.Get("LDAP.Passwrod"));
			

			DirectoryEntry obDirEntry = new DirectoryEntry(ldapPath, ldapUserName, ldapPassword);
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
                obUser.Properties["userPrincipalName"].Value = strLogin + "@Edge.BI"; 
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
        PASSWD_CANT_CHANGE=0x0040,
		UF_DONT_EXPIRE_PASSWD = 0x10000,
		UF_ACCOUNTDISABLE = 0x00002
	}
}

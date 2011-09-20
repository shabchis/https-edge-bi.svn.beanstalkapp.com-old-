

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using System.ServiceModel;
using myFacebook = Facebook;
using Easynet.Edge.Services.DataRetrieval.Retriever;
//using Excel =  Microsoft.Office.Interop.Excel;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;
using System.Data;
using System.Net;



using System.IO;





namespace Easynet.Edge.Services.Facebook
{
	public class FacebookTargeting : FacebookBaseRetriever
	{


		protected override bool SaveReport()
		{
			GetReportData();


			return true;
		}


		protected bool UpdateDB(string adgroup, string ageMin, string ageMax, string birthday,
			string languages, string workplace, string sex, string relation,
			string education, string countries, string keywords)
		{
			using (Easynet.Edge.Core.Data.DataManager.Current.OpenConnection())
			{
				try
				{

					SqlCommand SPCmd = DataManager.CreateCommand(@"FacebookAddTargeting(@AdGroupID:BigInt,
                    @MinAge:int,@MaxAge:Int,@Birthday:Int, @Sex:int, @Relationship:Int,@Languages:NVarChar,
                  @Education:Int,@Workplaces:NVarChar,@Location:NVarChar,@Keywords:NVarChar,@Channel:Int)",
							  CommandType.StoredProcedure);


					SPCmd.CommandTimeout = 120;
					SPCmd.Parameters["@AdGroupID"].Value = Convert.ToInt64(adgroup);
					SPCmd.Parameters["@MinAge"].Value = Convert.ToInt32(ageMin);
					SPCmd.Parameters["@MaxAge"].Value = Convert.ToInt32(ageMax);
					SPCmd.Parameters["@Birthday"].Value = Convert.ToInt32(birthday);
					SPCmd.Parameters["@Sex"].Value = Convert.ToInt32(sex);
					SPCmd.Parameters["@Relationship"].Value = Convert.ToInt32(relation);
					SPCmd.Parameters["@Languages"].Value = languages;
					SPCmd.Parameters["@Education"].Value = Convert.ToInt32(education);
					SPCmd.Parameters["@Workplaces"].Value = workplace;



					SPCmd.Parameters["@Location"].Value = countries;
					SPCmd.Parameters["@keywords"].Value = keywords;
					SPCmd.Parameters["@Channel"].Value = 6;

					object result = SPCmd.ExecuteScalar();

					return true;

				}
				catch (Exception ex)
				{
					Core.Utilities.Log.Write("Failed to write keepalive time to DB.", ex);
					return false;
				}
			}
		}

		protected override void GetReportData()
		{


			myFacebook.Rest.Api _facebookAPI = new myFacebook.Rest.Api(connSession);

			//Dictionary<string, string> parameterList = new Dictionary<string, string>();





			//  getAdGroupTargeting Data
			// parameterList.Clear();


			//parameterList.Add("method", "facebook.Auth.createToken");
			//string xx = _facebookAPI.Application.SendRequest(parameterList);
			//xx = xx.Replace("xsd:", "");
			//System.Xml.XmlDocument xmlTargetxx= new System.Xml.XmlDocument();
			//xmlTargetxx.LoadXml(xx);




			//_facebookAPI.Auth.Session.SessionExpires = false;
			////  getAdGroupTargeting Data
			//parameterList.Clear();
			//parameterList.Add("include_deleted", "true");
			//parameterList.Add("adgroup_ids", "");
			//parameterList.Add("campaign_ids", "");
			//parameterList.Add("account_id", _FBaccountID);
			//parameterList.Add("method", "facebook.Ads.getAdGroupTargeting");
			string url = string.Format("method/ads.getAdGroupTargeting?account_id={0}&include_deleted={1}", _FBaccountID, true);

			string res4 = SendFacebookRequest(url, "getAdGroupTargeting");
			res4 = res4.Replace("xsd:", "");
			System.Xml.XmlDocument xmlTargeting = new System.Xml.XmlDocument();
			xmlTargeting.LoadXml(res4);

			int xmlTargetingount = xmlTargeting.ChildNodes[1].ChildNodes.Count;
			List<Dictionary<string, System.Xml.XmlNode>> ListOfTargets = new List<Dictionary<string, System.Xml.XmlNode>>();


			for (int i = 0; i < xmlTargetingount; i++)
			{
				//  Dictionary<string, System.Xml.XmlNode> newItemTarget = new Dictionary<string, System.Xml.XmlNode>();
				//  newItemTarget.Add(xmlTargeting.ChildNodes[1].ChildNodes[i].ChildNodes[0].InnerText, xmlTargeting.ChildNodes[1].ChildNodes[i]);

				string ageMax, ageMin, sex, birthday, adgroup;
				adgroup = "0";
				ageMax = "0";
				ageMin = "0";
				birthday = "0";
				sex = "0";
				string education = "0";
				string countries = string.Empty;
				string languages = string.Empty;
				string keywords = string.Empty;
				string workplace = string.Empty;
				string relation = "0";

				foreach (var childNode in xmlTargeting.ChildNodes[1].ChildNodes[i].ChildNodes)
				{




					string NodeName = ((System.Xml.XmlNode)childNode).Name;
					switch (NodeName)
					{
						case "keywords":
							foreach (var node in ((System.Xml.XmlNode)childNode).ChildNodes)
							{
								keywords = keywords + "," + ((System.Xml.XmlNode)node).ChildNodes[0].Value;
							}
							break;
						case "countries":
							foreach (var node in ((System.Xml.XmlNode)childNode).ChildNodes)
							{
								countries = countries + "|" + ((System.Xml.XmlNode)node).ChildNodes[0].Value;
							}
							break;
						case "age_max":
							ageMax = ((System.Xml.XmlNode)childNode).ChildNodes[0].Value;
							break;
						case "age_min":
							ageMin = ((System.Xml.XmlNode)childNode).ChildNodes[0].Value;
							break;
						case "genders":
							if ((((System.Xml.XmlNode)childNode).ChildNodes.Count == 2))
							{
								sex = "3";
								break;
							}
							else foreach (var node in ((System.Xml.XmlNode)childNode).ChildNodes)
								{
									sex = ((System.Xml.XmlNode)node).ChildNodes[0].Value;
								}

							break;
						case "education_statuses":
							foreach (var node in ((System.Xml.XmlNode)childNode).ChildNodes)
							{
								education = ((System.Xml.XmlNode)childNode).ChildNodes[0].Value;
							}
							break;
						case "adgroup_id":
							adgroup = ((System.Xml.XmlNode)childNode).ChildNodes[0].Value;
							break;
					}
				}


				//ListOfTargets.Add(newItemTarget);
				if (countries.Length > 0)
					countries = countries.Remove(0, 1);

				if (keywords.Length > 0)
					keywords = keywords.Remove(0, 1);
				UpdateDB(adgroup, ageMin, ageMax, birthday, languages, workplace, sex, relation, education, countries, keywords);
			}

		}


	}
}
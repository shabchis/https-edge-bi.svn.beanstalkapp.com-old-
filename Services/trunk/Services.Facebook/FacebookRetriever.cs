﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using System.ServiceModel;
using myFacebook = Facebook;
using Easynet.Edge.Services.DataRetrieval.Retriever;
//using Excel =  Microsoft.Office.Interop.Excel;
using System.Xml;

namespace Easynet.Edge.Services.Facebook
{
	public class FacebookRetriever : FacebookBaseRetriever
	{

		protected List<FacebookRow> listOfFaceBookRows;
		protected List<FacebookRow> listOfRows = new List<FacebookRow>();
		protected List<AdGroupClass> listOfAdGroup;
		public List<CampaignClass> campaignList;

		protected override void GetReportData()
		{


			//myFacebook.Rest.Api _facebookAPI = new myFacebook.Rest.Api(connSession);

			//_facebookAPI.Auth.Session.SessionExpires = false;


			Dictionary<string, string> parameterList = new Dictionary<string, string>();

			string url;


			//=============================================================================               
			//GettAdGroupStats
			//parameterList.Add("adgroup_ids", "");
			//parameterList.Add("include_deleted", "true");
			//parameterList.Add("campaign_ids", "");


			//parameterList.Add("account_id", _FBaccountID);
			//parameterList.Add("method", "facebook.Ads.getAdGroupStats");

			// _requiredDay = new DateTime(2009, 11, 11, 1, 1, 1);

			//Adding 10 hours because the time differences between Israel local time and CA,USA time
			DateTime fromDate = this._requiredDay.AddHours(10);
			DateTime toDate = this._requiredDay.AddDays(1).AddHours(10);





			long fromLongDate = myFacebook.Utility.DateHelper.ConvertDateToFacebookDate(fromDate);
			long toLongDate = myFacebook.Utility.DateHelper.ConvertDateToFacebookDate(toDate);

			//***** test
			//  fromLongDate = 0;
			//  toLongDate = 0;
			//***** test
			string date = "[{\"time_start\":" + fromLongDate + ",\"time_stop\":" + toLongDate + "}]";
			// parameterList.Add("time_ranges", date);
			url = string.Format("method/ads.getAdGroupStats?account_id={0}&include_deleted={1}&stats_mode={2}&time_ranges={3}", _FBaccountID, true, "with_delivery", date);
			string res = SendFacebookRequest(url,"getAdGroupStats");
			System.Xml.XmlDocument getAdGroupStatsXmlDoc = new System.Xml.XmlDocument();
			getAdGroupStatsXmlDoc.LoadXml(res);

			int getAdGroupStatsCount = getAdGroupStatsXmlDoc.ChildNodes[1].ChildNodes[0].ChildNodes[2].ChildNodes.Count;
			//~GettAdGroupStats
			//======================================================================================

			//  Core.Utilities.Log.Write("AAAAAAAAAAAAA " + getAdGroupStatsXmlDoc.OuterXml, Core.Utilities.LogMessageType.Error);


			//======================================================================================
			//GetAdGroups Data
			//parameterList.Clear();
			//parameterList.Add("adgroup_ids", "");
			//parameterList.Add("include_deleted", "true");
			//parameterList.Add("campaign_ids", "");
			//parameterList.Add("account_id", _FBaccountID);
			//parameterList.Add("method", "facebook.Ads.getAdGroups");

			url = string.Format("method/ads.getAdGroups?account_id={0}&include_deleted={1}", _FBaccountID, true);
			System.Xml.XmlDocument getAdGroupsXmlDoc = new System.Xml.XmlDocument();
			string getAdGroupsRes = SendFacebookRequest(url, "getAdGroups");
			getAdGroupsXmlDoc.LoadXml(getAdGroupsRes);

			int count = getAdGroupsXmlDoc.ChildNodes[1].ChildNodes.Count;
			List<Dictionary<string, System.Xml.XmlNode>> ListOfAdGroupds = new List<Dictionary<string, System.Xml.XmlNode>>();

			if (count == 0)
			{
				Core.Utilities.Log.Write("Empty report from facebook ", Core.Utilities.LogMessageType.Warning);
				return;
			}
			for (int i = 0; i < count; i++)
			{
				Dictionary<string, System.Xml.XmlNode> newItem = new Dictionary<string, System.Xml.XmlNode>();
				newItem.Add(getAdGroupsXmlDoc.ChildNodes[1].ChildNodes[i].ChildNodes[0].InnerText, getAdGroupsXmlDoc.ChildNodes[1].ChildNodes[i]);
				ListOfAdGroupds.Add(newItem);
			}
			//~GetAdGroups Data
			//======================================================================


			//======================================================================
			//GetCampain Data
			//parameterList.Clear();
			//parameterList.Add("include_deleted", "true");
			//parameterList.Add("campaign_ids", "");
			//parameterList.Add("account_id", _FBaccountID);
			//parameterList.Add("method", "facebook.Ads.getCampaigns");


			url = string.Format("method/ads.getCampaigns?account_id={0}&include_deleted={1}", _FBaccountID, true);

			string getCampaignsRes = SendFacebookRequest(url, "getCampaigns");
			System.Xml.XmlDocument xmlCampaing = new System.Xml.XmlDocument();
			xmlCampaing.LoadXml(getCampaignsRes);

			int xmlCampaingCount = xmlCampaing.ChildNodes[1].ChildNodes.Count;
			List<Dictionary<string, System.Xml.XmlNode>> ListOfCampaigns = new List<Dictionary<string, System.Xml.XmlNode>>();

			//...........
			// for API bug 2011-03-21
			List<string> campaignArrays = new List<string>();
			string campaignArray = null;
			int campaignArrayIndex = 0;
			int maxCampaignsPerRequest = Instance.Configuration.Options["MaxCampaignsForAdGroupCreativesReport"] != null ?
				Int32.Parse(Instance.Configuration.Options["MaxCampaignsForAdGroupCreativesReport"]) :
				Int32.MaxValue;
			//...........

			for (int i = 0; i < xmlCampaingCount; i++)
			{
				Dictionary<string, System.Xml.XmlNode> newItem = new Dictionary<string, System.Xml.XmlNode>();
				string crazyNoam_campaignID = xmlCampaing.ChildNodes[1].ChildNodes[i].ChildNodes[1].InnerText;
				newItem.Add(crazyNoam_campaignID, xmlCampaing.ChildNodes[1].ChildNodes[i]);
				ListOfCampaigns.Add(newItem);

				//...........
				// for API bug 2011-03-21
				if (campaignArray == null)
					campaignArray = "[";
				campaignArray += (campaignArray.Length > 1 ? "," : "") + crazyNoam_campaignID;
				campaignArrayIndex++;
				if (campaignArrayIndex == maxCampaignsPerRequest || i == xmlCampaingCount - 1)
				{
					campaignArray += "]";
					campaignArrayIndex = 0;
					campaignArrays.Add(campaignArray);
					campaignArray = null;
				}
				//...........
			}

			//~ //GetCampain Data
			//======================================================================


			//======================================================================               
			//GetAdGroupCreatives Data

			List<Dictionary<string, System.Xml.XmlNode>> ListOfCreative = new List<Dictionary<string, System.Xml.XmlNode>>();

			for (int j = 0; j < campaignArrays.Count; j++)
			{
				//parameterList.Clear();
				//parameterList.Add("include_deleted", "true");
				//parameterList.Add("campaign_ids", campaignArrays[j]);
				//parameterList.Add("account_id", _FBaccountID);
				//parameterList.Add("method", "facebook.Ads.getAdGroupCreatives");

				url = string.Format("method/ads.getAdGroupCreatives?account_id={0}&include_deleted={1}&{2}", _FBaccountID, true, string.Format("campaign_ids={0}", campaignArrays[j]));
				string res3 = SendFacebookRequest(url, "getAdGroupCreatives", "-" + (j + 1).ToString());
				System.Xml.XmlDocument xmlCreative = new System.Xml.XmlDocument();
				xmlCreative.LoadXml(res3);

				int xmlCreativeCount = xmlCreative.ChildNodes[1].ChildNodes.Count;
				for (int i = 0; i < xmlCreativeCount; i++)
				{
					//  xmlCreative.ChildNodes[1].ChildNodes[i].OuterXml
					Dictionary<string, System.Xml.XmlNode> newItem = new Dictionary<string, System.Xml.XmlNode>();
					//   System.Xml.XmlNode sr =  xmlCreative.ChildNodes[1].ChildNodes[i].SelectSingleNode("/adgroup_id");
					string adgourp_id = FindIAdgroupID(xmlCreative.ChildNodes[1].ChildNodes[i]);
					//  var elements = System.Xml.Linq.XDocument.Parse(xmlCreative.ChildNodes[1].ChildNodes[i].OuterXml).Descendants("adgroup_id").Select(e => e.Value);


					newItem.Add(adgourp_id, xmlCreative.ChildNodes[1].ChildNodes[i]);
					ListOfCreative.Add(newItem);
				}
			}
			//~GetAdGroupCreatives Data
			//======================================================================



			//  getAdGroupTargeting Data
			//parameterList.Clear();
			//parameterList.Add("include_deleted", "true");
			//parameterList.Add("adgroup_ids", "");
			//parameterList.Add("campaign_ids", "");
			//parameterList.Add("account_id", _FBaccountID);
			//parameterList.Add("method", "facebook.Ads.getAdGroupTargeting");
			//string res4 = _facebookAPI.Application.SendRequest(parameterList);
			//res4 = res4.Replace("xsd:", "");
			//System.Xml.XmlDocument xmlTargeting = new System.Xml.XmlDocument();
			//xmlTargeting.LoadXml(res4);

			//int xmlTargetingount = xmlTargeting.ChildNodes[1].ChildNodes.Count;
			//List<Dictionary<string, System.Xml.XmlNode>> ListOfTargets = new List<Dictionary<string, System.Xml.XmlNode>>();
			//for (int i = 0; i < xmlTargetingount; i++)
			//{
			//    Dictionary<string, System.Xml.XmlNode> newItemTarget = new Dictionary<string, System.Xml.XmlNode>();
			//    newItemTarget.Add(xmlCreative.ChildNodes[1].ChildNodes[i].ChildNodes[0].InnerText, xmlCreative.ChildNodes[1].ChildNodes[i]);
			//    ListOfCreative.Add(newItemTarget);
			//}
			//~getAdGroupTargeting Data
			//======================================================================

			listOfAdGroup = new List<AdGroupClass>();
			string adGroupID, campaignID;

			XmlNamespaceManager xpathManager = new XmlNamespaceManager(getAdGroupStatsXmlDoc.NameTable);
			xpathManager.AddNamespace("fb", getAdGroupStatsXmlDoc.DocumentElement.NamespaceURI);


			listOfFaceBookRows = new List<FacebookRow>();

			foreach (System.Xml.XmlNode node in getAdGroupStatsXmlDoc.DocumentElement.SelectNodes("//fb:ads_stats", xpathManager))
			{
				//  getAdGroupStatsXmlDoc.Load(@"c:\dt.txt");
				Easynet.Edge.Services.Facebook.FacebookRow newRow = new FacebookRow();

				foreach (System.Xml.XmlElement innerChild in node.ChildNodes)
				{
					string fieldName = innerChild.LocalName;
					if (fieldName.Equals("id"))
					{
						adGroupID = innerChild.InnerText;

						//run on Creative results
						for (int g = 0; g < ListOfCreative.Count; g++)
						{
							if (ListOfCreative[g].Keys.First().Equals(adGroupID))
							{
								foreach (System.Xml.XmlNode innerACreativeChilds in ListOfCreative[g].Values.First().ChildNodes)
								{
									if (rawDataFields.Fields[innerACreativeChilds.Name] != null)
									{
										if (rawDataFields.Fields[innerACreativeChilds.Name].Enabled == true)
										{
											//Bug Fix - "Cannot get Adgroup Name"(Due to Facebook API Changes 06.03.2011 ) 
											if (!(rawDataFields.Fields[innerACreativeChilds.Name].Key.ToString().Equals("name")))
											{
												string value = innerACreativeChilds.InnerText.Replace("\t", string.Empty);
												value = value.Replace("\n", string.Empty);
												value = value.Replace("\r", string.Empty);
												newRow._Values.Add(rawDataFields.Fields[innerACreativeChilds.Name].Value, value);
											}
										}
									}
								}
								break;

							}
						}
						//~run on Creative results


						//run on AdGroups results
						for (int h = 0; h < ListOfAdGroupds.Count; h++)
						{
							if (ListOfAdGroupds[h].Keys.First().Equals(adGroupID))
							{
								foreach (System.Xml.XmlNode innerAdGroupChilds in ListOfAdGroupds[h].Values.First().ChildNodes)
								{
									if (innerAdGroupChilds.Name.Equals("campaign_id"))
									{
										campaignID = innerAdGroupChilds.InnerText;

										for (int j = 0; j < ListOfCampaigns.Count; j++)
										{
											if (ListOfCampaigns[j].Keys.First().Equals(campaignID))
											{
												foreach (System.Xml.XmlNode innerCampaignChilds in ListOfCampaigns[j].Values.First().ChildNodes)
												{
													if (rawDataFields.Fields[innerCampaignChilds.Name] != null)
													{
														if (rawDataFields.Fields[innerCampaignChilds.Name].Enabled == true)
														{
															if (innerCampaignChilds.Name.Equals("name"))
															{
																newRow._Values.Add(rawDataFields.Fields["campaign name"].Value, innerCampaignChilds.InnerText);
															}
															else
																newRow._Values.Add(rawDataFields.Fields[innerCampaignChilds.Name].Value, innerCampaignChilds.InnerText);
														}
													}

												}

											}
										}
									}



									if (rawDataFields.Fields[innerAdGroupChilds.Name] != null)
									{
										if (rawDataFields.Fields[innerAdGroupChilds.Name].Enabled == true)
										{
											if (newRow._Values.Keys.Contains<string>(rawDataFields.Fields[innerAdGroupChilds.Name].Value) == false)
												if (innerAdGroupChilds.Name.Equals("name"))
												{


													// PIPE handleing (&) part
													if (innerAdGroupChilds.InnerText.IndexOf(_pipe) > -1)
													{
														string[] arr = innerAdGroupChilds.InnerText.Split(_pipe.ToCharArray());


														newRow._Values.Add(rawDataFields.Fields[innerAdGroupChilds.Name].Value, arr[0]);
														newRow._Values.Add("adName", arr[1]);
														// newRow._Values.Add(rawDataFields.Fields[innerAdGroupChilds.Name].Value, innerAdGroupChilds.InnerText.Substring(0, innerAdGroupChilds.InnerText.Length - innerAdGroupChilds.InnerText.IndexOf("@")));

													}
													else
													{
														newRow._Values.Add(rawDataFields.Fields[innerAdGroupChilds.Name].Value, innerAdGroupChilds.InnerText);
														newRow._Values.Add("adName", "");

														// newRow._Values.Add(rawDataFields.Fields[innerAdGroupChilds.Name].Value, innerAdGroupChilds.InnerText);
													}
												}
												else
													newRow._Values.Add(rawDataFields.Fields[innerAdGroupChilds.Name].Value, innerAdGroupChilds.InnerText);
										}
									}

								}
								break;
							}
						}

					}

					//run on AdGroupsStats results
					if (rawDataFields.Fields[fieldName] != null)
					{
						if (rawDataFields.Fields[fieldName].Enabled == true)
						{
							newRow._Values.Add(rawDataFields.Fields[fieldName].Value, innerChild.InnerText);
						}
					}
				}
				listOfFaceBookRows.Add(newRow);
			}

			// Core.Utilities.Log.Write("---------------listOfFaceBookRows.Count, " + listOfFaceBookRows.Count, Core.Utilities.LogMessageType.Error);
		}


		protected string FindIAdgroupID(System.Xml.XmlNode node)
		{
			XmlElement creative = (XmlElement)node;
			if (!creative.HasAttribute("key"))
				throw new Exception("Adgroup creative is missing attribute 'key' in facebook.Ads.getAdGroupCreatives report.");

			return creative.GetAttribute("key");
			/*
            foreach (System.Xml.XmlNode _node in node.ChildNodes)
            {
                if (_node.Name.Equals("adgroup_id"))
                    return _node.InnerText;
               
            }
			*/
			return ""; ;
		}
		protected override bool SaveReport()
		{
			GetReportData();





			string fileName = WriteResultToFile();




			if (null == fileName)
			{
				Core.Utilities.Log.Write("Saved report file name is empty!!  Date:" + _requiredDay, Core.Utilities.LogMessageType.Information);
				return true;
			}

			if ("Zero resaults".Equals(fileName))
			{
				Core.Utilities.Log.Write("Saved report file name is empty!!  Date:" + _requiredDay, Core.Utilities.LogMessageType.Information);
				return true;
			}
			Core.Utilities.Log.Write("fileName: " + fileName, Core.Utilities.LogMessageType.Information);
			//string fileName = WriteResultToFile(dataFromBO, _requiredDay);
			if (!String.IsNullOrEmpty(fileName))
				return SaveFilePathToDB(serviceType, fileName, _requiredDay, _adwordsFile);
			else
			{
				Core.Utilities.Log.Write("Saved report file name is empty.", Core.Utilities.LogMessageType.Information);
				return false;
			}
		}

		protected override string WriteResultToFile()
		{
			if (null == listOfFaceBookRows)
				return null;
			//Core.Utilities.Log.Write("---------------listOfFaceBookRows.Count, " + listOfFaceBookRows.Count, Core.Utilities.LogMessageType.Error);
			if (listOfFaceBookRows.Count > 0)
				return CreateXLSFile(listOfFaceBookRows);
			else
				Core.Utilities.Log.Write("Zero resaults ", Core.Utilities.LogMessageType.Information);
			{
				return "Zero resaults";
			}

		}


	}
}

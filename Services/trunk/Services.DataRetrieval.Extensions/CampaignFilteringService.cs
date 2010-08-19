using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Services.DataRetrieval.Processor;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;
using System.IO;
using System.Xml;
using System.Threading;


namespace Easynet.Edge.Services.DataRetrieval.Extensions
{
    public class CampaignFilteringService: BaseProcessor
    {
        string _BackupFile = "";
        protected override ServiceOutcome DoWork()
        {
            bool rangeDate = false;
            ArrayList al = new ArrayList();
            InitalizeServiceData(ref rangeDate, al);
            for (int i = 0; i < al.Count; i++)
            {
                backupXML(al[i]);
                filterXml(al[i]);
            }

            return ServiceOutcome.Success;
        }
        private void backupXML(object file)
        {
            string backupPath = Instance.ParentInstance.Configuration.Options["BackupFilteringPath"];
            _BackupFile = backupPath +  "\\" + System.IO.Path.GetFileName((string)file);
            try
            {
                if (!System.IO.Directory.Exists(backupPath))
                    System.IO.Directory.CreateDirectory(backupPath);
                if (System.IO.File.Exists(_BackupFile))
                {
                    System.IO.File.Delete(_BackupFile);
                }
                //checking if file is not in user by another process
                while (true)
                {
                    try
                    {
                        XmlTextReader reader = new XmlTextReader((string)file);
                        break;
                    }
                    catch (IOException)
                    {
                    }
                    catch (Exception)
                    {
                        break;
                    }
                } 

                System.IO.File.Move((string)file, _BackupFile);
            }
            catch (Exception ex)
            {
                throw new Exception("error accessing " + _BackupFile + " file", ex);
            }
        }
        private bool checkValidation(ref string[] campaignsList, string campaignIds, string campaignsNames)
        {
            bool isCampaignId = false;
            double result;

            if (campaignIds != null)
            {
                isCampaignId = true;
                campaignsList = campaignIds.Split("|".ToCharArray());
            }
            else if (campaignsNames != null)
            {
                isCampaignId = false;
                campaignsList = campaignsNames.Split("|".ToCharArray());
            }
            else
            {
                throw new Exception("No Campaign Names/Ids mentioned.");
            }
            if (isCampaignId)
            {
                foreach (string str in campaignsList)
                {
                    if (!double.TryParse(str, out result))
                    {
                        throw new Exception("Wrong campaign Ids.");
                    }
                }
            }

            return isCampaignId;
        }
        private void filterXml(object ob)
        {
            string campaignsNames = Instance.ParentInstance.Configuration.Options["Campaigns"];
            string campaignIds = Instance.ParentInstance.Configuration.Options["CampaignIds"];
            string[] campaignsList = null;

            bool isCampaignId = false;

            isCampaignId = checkValidation(ref campaignsList, campaignIds, campaignsNames);
            XmlTextReader reader = null;
            //check if file is not in use by the backup process
            while (true)
            {
                try
                {
                    reader = new XmlTextReader(_BackupFile);
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(2000);
                }
                catch (Exception)
                {
                    break;
                }
            } 

            
            reader.WhitespaceHandling = WhitespaceHandling.None;
            XmlTextWriter writer = new XmlTextWriter((string)ob, null);
            string campaignName;
            string campaignId;
        	reader.Read();

			// Loop on all the rows in the xml report file.
            while (!reader.EOF)
            {
                if (reader.NodeType != XmlNodeType.EndElement)
                {
                    //first two nodes are - columns and xml
                    if (reader.Name.ToLower().Equals("columns") || reader.Name.ToLower().Equals("xml") || reader.Name.ToLower().Equals("totals"))
                    {
                        writer.WriteNode(reader, true);

                        continue;
                    }
                    //open an element for every node which is not row node
                    else if (!reader.Name.ToLower().Equals("row"))
                    {
                        writer.WriteStartElement(reader.Name, "");
                        reader.Read();
                        continue;
                    }
                    //campaign ids in configuration
                    if (isCampaignId && reader.Name.ToLower().Equals("row") && (reader.GetAttribute("campaignid") != null))
                    {
                        campaignId = reader.GetAttribute("campaignid").ToString();
                        if (campaignsList.Contains(campaignId))
                        {
                            writer.WriteNode(reader, true);
                        }
                        else
                            reader.Read();
                    }
                    //campaign names in configuration
                    else if ((!isCampaignId) && reader.Name.ToLower().Equals("row") && (reader.GetAttribute("campaign") != null))
                    {
                        campaignName = reader.GetAttribute("campaign").ToString();
                        if (campaignsList.Contains(campaignName))
                        {
                            writer.WriteNode(reader, true);
                        }
                        else
                            reader.Read();
                    }
                    else
                        reader.Read();
                }
                else if (!reader.Name.ToLower().Equals("row") && reader.NodeType == XmlNodeType.EndElement)
                {
                    writer.WriteEndElement();
                    reader.Read();
                }
                else
                    reader.Read();
            }
            writer.Close();
            reader.Close();
        }
        protected bool GetToRowsSection(XmlReader xmlReader, XmlWriter xmlWriter)
        {
            if (xmlReader.Name.ToLower() != "row" || !xmlReader.HasAttributes)
            {
                return false;
            }
            else
                return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AnalysisServices;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Collections;

namespace EdgeBI.Wizards.AccountWizard.CubeCreation
{
    public class CubeCreation : CubeHandler
    {
        bool found = false;
        XmlDocument _xd = new XmlDocument();
        private void CubeMeasuresUpdate(XmlNode xn, Dictionary<string, string> measures)
        {
            xn = _xd.SelectSingleNode("//MeasureGroup");
            XmlNodeList xnl = null;
            string tempID = string.Empty;

            xnl = xn.ChildNodes;
            xn = xnl[2].ChildNodes[0];
            while (xn != null && xn.Name.Equals("Measure"))
            {
                if (xn.InnerXml.StartsWith("<ID>Client Specific"))
                {
                    for (int i = 0; i < xn.ChildNodes.Count; i++)
                    {
                        if (xn.ChildNodes[i].Name.ToLower().Equals("id") && measures.ContainsKey(xn.ChildNodes[i].FirstChild.Value))
                        {
                            tempID = xn.ChildNodes[i].FirstChild.Value;
                            found = true;
                        }
                        if (xn.ChildNodes[i].Name.ToLower().Equals("name") && found)
                        {
                            xn.ChildNodes[i].FirstChild.Value = measures[tempID];
                            found = false;
                        }
                    }
                }
                if (xn.NextSibling == null)
                {
                    try
                    {
                        xn = xn.ParentNode.ParentNode;
                        xn = xn.NextSibling;
                        xn = xn.ChildNodes[2].ChildNodes[0];
                    }
                    catch
                    { }
                }
                else
                    xn = xn.NextSibling;
            }
        }
        private void CubeCalculatedMembersUpdate(string cpa1, string cpa2, string cpa1ToReplace, string cpa2ToReplace, Dictionary<string, string> measures, bool replaceCPA, Dictionary<string, string> strings)
        {
            string value = string.Empty;
            XmlNode xn = null;
            xn = _xd.SelectSingleNode("//Text");
            value = xn.ChildNodes[0].Value;

            if (replaceCPA)
            {
                if (!cpa1ToReplace.Equals(""))
                {
                    string sPattern = cpa1;
                    int index = 0;
                    while (index < value.Length)
                    {
                        if (index == -1)
                            break;
                        index = value.ToLower().IndexOf(sPattern.ToLower(), index);
                        if (index == -1)
                            break;
                        char ch = value.Substring(index + sPattern.Length, 1)[0];
                        char formerCh = value.Substring(index - 1, 1)[0];
                        if (!char.IsLetter(ch) && !char.IsNumber(ch) && !char.IsLetter(formerCh) && !char.IsNumber(formerCh))
                        {
                            value = value.Substring(0, index) + cpa1ToReplace + value.Substring(index + sPattern.Length);
                        }
                    }

                    //value = value.ToLower().Replace(cpa1, cpa1ToReplace);
                }
                if (!cpa2ToReplace.Equals(""))
                {
                    string sPattern = cpa2;
                    int index = 0;
                    while (index < value.Length)
                    {
                        if (index == -1)
                            break;
                        index = value.ToLower().IndexOf(sPattern.ToLower(), index);
                        if (index == -1)
                            break;
                        char ch = value.Substring(index + sPattern.Length, 1)[0];
                        char formerCh = value.Substring(index - 1, 1)[0];
                        if (!char.IsLetter(ch) && !char.IsNumber(ch) && !char.IsLetter(formerCh) && !char.IsNumber(formerCh))
                        {
                            value = value.Substring(0, index) + cpa2ToReplace + value.Substring(index + sPattern.Length);
                        }
                    }
                    //value = value.ToLower().Replace(cpa2, cpa2ToReplace);
                }
            }
            //replace in calculated members
            IDictionaryEnumerator ie = measures.GetEnumerator();
            while (ie.MoveNext())
            {
                string sPattern = ((KeyValuePair<string, string>)(ie.Current)).Key;
                int index = 0;
                while (index < value.Length)
                {
                    if (index == -1)
                        break;
                    index = value.ToLower().IndexOf(sPattern.ToLower(),index);
                    if (index == -1)
                        break;
                    char ch = value.Substring(index + sPattern.Length, 1)[0];
                    char formerCh = value.Substring(index - 1, 1)[0];
                    if (!char.IsLetter(ch) && !char.IsNumber(ch) && !char.IsLetter(formerCh) && !char.IsNumber(formerCh))
                    {
                        value = value.Substring(0, index) + ((KeyValuePair<string, string>)(ie.Current)).Value + value.Substring(index + sPattern.Length);
                        //value = value.Replace(((KeyValuePair<string, string>)(ie.Current)).Key, ((KeyValuePair<string, string>)(ie.Current)).Value);
                        //value = value.Replace(((KeyValuePair<string, string>)(ie.Current)).Key.ToLower(), ((KeyValuePair<string, string>)(ie.Current)).Value);
                    }
                }
            }

            ie = strings.GetEnumerator();
            while (ie.MoveNext())
            {
                string sPattern = ((KeyValuePair<string, string>)(ie.Current)).Key;
                int index = 0;
                while (index < value.Length)
                {
                    if (index == -1)
                        break;
                    index = value.ToLower().IndexOf(sPattern.ToLower(), index);
                    if (index == -1)
                        break;
                    char ch = value.Substring(index + sPattern.Length, 1)[0];
                    char formerCh = value.Substring(index - 1, 1)[0];
                    if (!char.IsLetter(ch) && !char.IsNumber(ch) && !char.IsLetter(formerCh) && !char.IsNumber(formerCh))
                    {
                        value = value.Substring(0, index) + ((KeyValuePair<string, string>)(ie.Current)).Value + value.Substring(index + sPattern.Length);
                        //value = value.Replace(((KeyValuePair<string, string>)(ie.Current)).Key, ((KeyValuePair<string, string>)(ie.Current)).Value);
                        //value = value.Replace(((KeyValuePair<string, string>)(ie.Current)).Key.ToLower(), ((KeyValuePair<string, string>)(ie.Current)).Value);
                    }
                }
            }
            xn.ChildNodes[0].Value = value;
        }
        public void CubeUpdate(string cubeTemplateId, string fileName, string roleId, string NewCubeName,
                    string databaseName, string dataSource, Database db, string cpa1,
                        string cpa2, string cpa1ToReplace, string cpa2ToReplace,
                            Dictionary<string, string> measures, string scopeID, Dictionary<string, string> strings)
        {
            Cube cb = new Cube();
            //XPathNavigator nav;
            XPathDocument docNav;
            //XPathNodeIterator NodeIter;
            //String strExpression; 
            bool replaceCPA = false;
            cb = getCubeObject(db, cubeTemplateId);
            //editing some cube object details
            cb.Name = NewCubeName;
            string databaseID = cb.Parent.ID;
            //MessageBox.Show(databaseID);
            string xmlString = string.Empty;
            xmlString = XmlFromCube(cb, fileName);
            string currDir = Directory.GetCurrentDirectory();
            docNav = new XPathDocument(currDir + "\\" + fileName);
            scopeIdUpdate(xmlString, scopeID);


            XmlNode xn = null;
            _xd.LoadXml(xmlString);
            xn = _xd.SelectSingleNode("//ID");
            xn.InnerText = NewCubeName;
            if (NewCubeName.ToLower().StartsWith("bo"))
                CubeMeasuresUpdate(xn, measures);
            if ((!cpa1ToReplace.Equals("") || !cpa2ToReplace.Equals("")))
                replaceCPA = true;
            if (NewCubeName.ToLower().StartsWith("bo"))
            {
                CubeCalculatedMembersUpdate(cpa1, cpa2, cpa1ToReplace, cpa2ToReplace, measures, replaceCPA, strings);
                //CubeCalculatedMembersUpdate(measures);
            }

            xn = _xd.SelectSingleNode("//CubePermissions");
            CubePermissionUpdate(xn, roleId);
            _xd.Save(currDir + "\\" + fileName);
            xmlString = addFirstNodeToXML(currDir + "\\" + fileName, databaseName, databaseID);
            //File.WriteAllText(currDir + "\\" + fileName, xmlString);
            CreateCubeFromXml(xmlString, dataSource);

            //SHOULD REMEMBER TO ADD THIS LINE TO END OF XMLA FILE
            //newFile.Append("<Create xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\">\n");
        }

        private void scopeIdUpdate(string xmlString, string scopeID)
        {
            int firstIndex = 0;
            int nextIndex = 0;
            while (firstIndex != -1)
            {
                firstIndex = xmlString.IndexOf("Scope_id", nextIndex);
                nextIndex = firstIndex + 1;
                if (firstIndex == -1)
                    break;
                int secondIndex = xmlString.IndexOf("<", firstIndex);
                string strToReplace = xmlString.Substring(firstIndex, secondIndex - firstIndex);
                xmlString.Replace(strToReplace, "Scope_id =" + scopeID);
            }
        }
        private void CubePermissionUpdate(XmlNode xn, string roleId)
        {
            XmlNode xnTemp;
            if (xn.ChildNodes.Count == 0)
            {
              //  MessageBox.Show("No roles defined in cube template.");
            }
            else if (xn.ChildNodes.Count > 1)
            {
                xnTemp = xn.LastChild;
                xn.InnerXml += xnTemp.OuterXml;
                xnTemp = xn.LastChild;
                for (int i = 0; i < xnTemp.ChildNodes.Count; i++)
                {
                    if (xnTemp.ChildNodes[i].Name.Equals("RoleID"))
                    {
                        xnTemp.ChildNodes[i].InnerXml = roleId;
                    }
                    else if (xnTemp.ChildNodes[i].Name.Equals("Name"))
                    {
                        xnTemp.ChildNodes[i].InnerXml = "CubePermission " + (xn.ChildNodes.Count - 1);
                    }
                    else if (xnTemp.ChildNodes[i].Name.Equals("ID"))
                    {
                        xnTemp.ChildNodes[i].InnerXml = "CubePermission " + (xn.ChildNodes.Count - 1);
                    }
                }

                //xnTemp.SelectSingleNode("//RoleID").Value = roleId;
                //xn.InnerXml += xnTemp.OuterXml;
                //xnTemp = null;
                //xnTemp = xn.ChildNodes[xn.ChildNodes.Count - 1];
                //xnTemp = xnTemp.SelectSingleNode("//RoleID");
                ////xnTemp.InnerXml = xnTemp.InnerXml + xnTemp.InnerXml;
                ////xnTemp = xnTemp.SelectSingleNode("//RoleID");
                //xnTemp.LastChild.Value = roleId;
                ////update permission id
                //xnTemp = ((xn).LastChild.SelectNodes("//ID")[xn.ChildNodes.Count - 1]);
                //xnTemp.LastChild.Value = "CubePermission " + (xn.ChildNodes.Count - 1);
                ////update permission name
                //xnTemp = xnTemp.SelectSingleNode("//Name");
                //xnTemp.LastChild.Value = "CubePermission " + (xn.ChildNodes.Count - 1);
            }
            //childNodes = 1
            else
            {
                xnTemp = xn.LastChild;
                xnTemp = xnTemp.SelectSingleNode("//RoleID");
                xnTemp.FirstChild.Value = roleId;
            }
        }

        private string addFirstNodeToXML(string filePath, string databaseName, string databaseID)
        {
            StringBuilder newFile = new StringBuilder();
            string temp = string.Empty;
            string[] file = File.ReadAllLines(filePath);

            newFile.Append("<Create xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\">\n");
            newFile.Append("<ParentObject>\n");
            newFile.Append("<DatabaseID>" + databaseID + "</DatabaseID>\n");
            newFile.Append("</ParentObject>\n");
            foreach (string line in file)
            {
                newFile.Append(line + "\n");
            }
            newFile.Append("</Create>");

            return newFile.ToString();
        }
    }
}

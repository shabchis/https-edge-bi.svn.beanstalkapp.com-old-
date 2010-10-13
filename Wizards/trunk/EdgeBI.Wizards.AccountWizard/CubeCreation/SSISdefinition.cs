using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace EdgeBI.Wizards.AccountWizard.CubeCreation
{
    public class SSISdefinition
    {
        public void SsisUpdate(string cubeName, string tempSource, string source, string CubeNamePatternToReplace, 
            string CubeNameReplaceWith, string DataBaseNamePatternToReplace, string DatabaseNameReplaceWith)
        {
            dtsxTemplateUpdate(tempSource, cubeName);
            dtsxUpdate(source, cubeName, CubeNamePatternToReplace, CubeNameReplaceWith, DataBaseNamePatternToReplace, DatabaseNameReplaceWith);

        }
        private void dtsxUpdate(string source, string cubeName, string CubeNamePatternToReplace, 
            string CubeNameReplaceWith, string DataBaseNamePatternToReplace, string DatabaseNameReplaceWith)
        {
            string nodeFromDb = "&lt;Process xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; xmlns:ddl2=&quot;http://schemas.microsoft.com/analysisservices/2003/engine/2&quot; xmlns:ddl2_2=&quot;http://schemas.microsoft.com/analysisservices/2003/engine/2/2&quot; xmlns:ddl100_100=&quot;http://schemas.microsoft.com/analysisservices/2008/engine/100/100&quot;&gt;&#xA;    &lt;Object&gt;&#xA;      &lt;DatabaseID&gt;Easynet_UDM&lt;/DatabaseID&gt;&#xA;      &lt;CubeID&gt;BOPPCCube888&lt;/CubeID&gt;&#xA;      &lt;MeasureGroupID&gt;BackOffice Client&lt;/MeasureGroupID&gt;&#xA;      &lt;PartitionID&gt;BackOffice_Gateways_2010&lt;/PartitionID&gt;&#xA;    &lt;/Object&gt;&#xA;    &lt;Type&gt;ProcessFull&lt;/Type&gt;&#xA;    &lt;WriteBackTableCreation&gt;UseExisting&lt;/WriteBackTableCreation&gt;&#xA;  &lt;/Process&gt;&#xA;  &lt;Process xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; xmlns:ddl2=&quot;http://schemas.microsoft.com/analysisservices/2003/engine/2&quot; xmlns:ddl2_2=&quot;http://schemas.microsoft.com/analysisservices/2003/engine/2/2&quot; xmlns:ddl100_100=&quot;http://schemas.microsoft.com/analysisservices/2008/engine/100/100&quot;&gt;&#xA;    &lt;Object&gt;&#xA;      &lt;DatabaseID&gt;Easynet_UDM&lt;/DatabaseID&gt;&#xA;      &lt;CubeID&gt;BOPPCCube888&lt;/CubeID&gt;&#xA;      &lt;MeasureGroupID&gt;PPC Campaigns&lt;/MeasureGroupID&gt;&#xA;      &lt;PartitionID&gt;PPC_Campaigns_2010&lt;/PartitionID&gt;&#xA;    &lt;/Object&gt;&#xA;    &lt;Type&gt;ProcessFull&lt;/Type&gt;&#xA;    &lt;WriteBackTableCreation&gt;UseExisting&lt;/WriteBackTableCreation&gt;&#xA;  &lt;/Process&gt;&#xA;  &lt;Process xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; xmlns:ddl2=&quot;http://schemas.microsoft.com/analysisservices/2003/engine/2&quot; xmlns:ddl2_2=&quot;http://schemas.microsoft.com/analysisservices/2003/engine/2/2&quot; xmlns:ddl100_100=&quot;http://schemas.microsoft.com/analysisservices/2008/engine/100/100&quot;&gt;&#xA;    &lt;Object&gt;&#xA;      &lt;DatabaseID&gt;Easynet_UDM&lt;/DatabaseID&gt;&#xA;      &lt;CubeID&gt;BOPPCCube888&lt;/CubeID&gt;&#xA;      &lt;MeasureGroupID&gt;PPC Campaign Targets&lt;/MeasureGroupID&gt;&#xA;      &lt;PartitionID&gt;Dwh_Fact_PPC_Campaign_Targets&lt;/PartitionID&gt;&#xA;    &lt;/Object&gt;&#xA;    &lt;Type&gt;ProcessFull&lt;/Type&gt;&#xA;    &lt;WriteBackTableCreation&gt;UseExisting&lt;/WriteBackTableCreation&gt;&#xA;  &lt;/Process&gt;";
            string patternToReplace = string.Empty;
            string updatedNode = string.Empty;
            string xmlString = string.Empty;
            string uniqueIdentifier = string.Empty;
            string secondUniqueIdentifier = string.Empty;

            updatedNode = nodeFromDb.Replace(CubeNamePatternToReplace, cubeName);
            if(!DataBaseNamePatternToReplace.ToLower().Trim().Equals(DatabaseNameReplaceWith.ToLower().Trim()))
                updatedNode = nodeFromDb.Replace(DataBaseNamePatternToReplace, DatabaseNameReplaceWith);

            TextReader r = new StreamReader(source);
            string input = String.Empty;
            while ((input = r.ReadLine()) != null)
            {
                xmlString += input;
            }
            r.Close();
            r.Dispose();

            uniqueIdentifier = "DTSID\">{AE9AC3B2-3233-4720-A3AA-611354208ECD";
            secondUniqueIdentifier = "xmlns=&quot;http://schemas.microsoft.com/analysisservices/2003/engine&quot;&gt;&#xA;";
            int index1 = xmlString.IndexOf(uniqueIdentifier);
            int index2 = xmlString.IndexOf(secondUniqueIdentifier, index1);
            index2 += secondUniqueIdentifier.Length;
            xmlString = xmlString.Insert(index2, updatedNode);
            File.WriteAllText(source, xmlString);
        }
        private void dtsxTemplateUpdate(string source, string cubeName)
        {
            string nodeString = string.Empty;
            string oldCubeName = string.Empty;
            XmlNode xn = null;
            string xmlString = string.Empty;
            int index1 = 0, index2 = 0;
            XmlDocument xd = new XmlDocument();
            TextReader r = new StreamReader(source);
            string input = String.Empty;
            while ((input = r.ReadLine()) != null)
            {
                xmlString += input;
            }
            r.Close();
            r.Dispose();


            index1 = xmlString.IndexOf("<ASProcessingData");
            index2 = xmlString.IndexOf("DTS:ObjectData>", index1);
            index2 += "DTS:ObjectData>".Length;
            while (!(xmlString.Substring(index1, index2 - index1).ToLower().Contains("cubeid")))
            {
                index1 = xmlString.IndexOf("<ASProcessingData",index2 + 1);
                index2 = xmlString.IndexOf("DTS:ObjectData>", index1);
                index2 += "DTS:ObjectData>".Length;
            }
            nodeString = xmlString.Substring(index1, index2 - index1);
            index1 = nodeString.ToLower().IndexOf(";cubeid&gt");
            index2 = nodeString.ToLower().IndexOf(";",index1 + 10);
            oldCubeName = nodeString.Substring(index1 + 11, index2 - index1 - 1);
            xmlString = xmlString.Replace(oldCubeName, cubeName);

            File.WriteAllText(source, xmlString);
        }
    }
}

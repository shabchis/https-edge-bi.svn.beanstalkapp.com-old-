using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.BusinessObjects;
using Easynet.Edge.Services.DataRetrieval;
using System.Collections;

namespace Easynet.Edge.Services.XmlFilter
{
    public class XmlFilterService : BaseService
    {
		/*
		protected override void GetReportPath(ArrayList filesPaths, string serviceType)
        {
			
			base.GetReportPath(filesPaths, serviceType);

			if (filesPaths.Count > 0)
				throw new Exception("No file path.");

			string xmlPath = filesPaths[0].ToString(); 
            //Copy the original file under a different name, and start editing the current file.
            string fileName = Path.GetDirectoryName(xmlPath) + @"\" + Path.GetFileNameWithoutExtension(xmlPath);
            fileName += "_original.xml";

            File.Move(xmlPath, fileName);

            //Start Editing.
            XmlDocument xd = new XmlDocument();
            xd.Load(fileName);

            XmlNodeList nodes = xd.SelectNodes("//row[@clicks>0]");

            XmlDocument output = new XmlDocument();
            XmlNode root = output.CreateNode(XmlNodeType.Element, "rows", String.Empty);

            foreach (XmlNode row in nodes)
            {
                XmlNode child = output.CreateNode(row.NodeType, row.Name, row.NamespaceURI);
                foreach (XmlAttribute xa in row.Attributes)
                {
                    XmlAttribute childAttrib = output.CreateAttribute(xa.Name);
                    childAttrib.Value = xa.Value;

                    child.Attributes.Append(childAttrib);
                }
                root.AppendChild(child);
            }

            output.AppendChild(root);
            output.Save(xmlPath);
        }
*/
		protected override ServiceOutcome DoWork()
		{
			//string xmlPath = string.Empty;
			//GetReportPath(ref xmlPath, "Google.Adwords");
			return ServiceOutcome.Success;
		}
    }
}

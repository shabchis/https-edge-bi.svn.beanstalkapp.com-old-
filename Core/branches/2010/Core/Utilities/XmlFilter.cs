using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Easynet.Edge.Core.Utilities
{

    public class XmlFilter
    {

        public static void Filter(string sourceFile, 
                                    string property,
                                    string lookupNode,
                                    string outputRootNode,
                                    string value,
                                    string op,
                                    string destinationFile)
        {

            if (sourceFile == null ||
                sourceFile == String.Empty)
                throw new ArgumentNullException("Invalid source file. Cannot be null/empty.");

            if (destinationFile == null ||
                destinationFile == String.Empty)
                throw new ArgumentNullException("Invalid destination file. Cannot be null/empty");

            if (op == String.Empty ||
                op == null)
                throw new ArgumentNullException("Invalid operator. Cannot be empty/null.");

            if (lookupNode == null ||
                lookupNode == String.Empty)
                throw new ArgumentNullException("Invalid node. Cannot be null/empty.");

            if (property == String.Empty ||
                property == null)
                throw new ArgumentNullException("Invalid property. Cannot be null/empty.");

            if (outputRootNode == null ||
                outputRootNode == String.Empty)
                throw new ArgumentNullException("Invalid output root node. Cannot be null/empty.");


            XmlDocument src = new XmlDocument();
            src.Load(sourceFile);

            XmlNodeList filtered = src.SelectNodes("//" + lookupNode + "[@" + property + op + value + "]");

            XmlDocument output = new XmlDocument();
            XmlNode root = output.CreateNode(XmlNodeType.Element, outputRootNode, String.Empty);

            foreach (XmlNode filteredRow in filtered)
            {
                XmlNode child = output.CreateNode(filteredRow.NodeType, 
                                                    filteredRow.Name, 
                                                    filteredRow.NamespaceURI);

                foreach (XmlAttribute attrib in filteredRow.Attributes)
                {
                    XmlAttribute childAttrib = output.CreateAttribute(attrib.Name);
                    childAttrib.Value = attrib.Value;

                    child.Attributes.Append(childAttrib);
                }

                root.AppendChild(child);
            }
        
            output.AppendChild(root);
            output.Save(destinationFile);
        }
    }
}

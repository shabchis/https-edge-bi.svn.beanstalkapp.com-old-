using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.BusinessObjects;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using Easynet.Edge.Services.DataRetrieval.DataReader;

namespace Easynet.Edge.Services.DataRetrieval.Processor
{
	class BackOfficeBabylonProcessorNodes : BackOfficeProcessorNodes
	{
		protected override void HandleBackOfficeNode(string nodeName, string nodeValue, FieldElementSection rawDataFields, SqlCommand insertCommand)
		{
			//If the node name is CODE, then we need to do some processing on the value.
			string boValue = nodeValue;
			if (nodeName.ToLower() == "code")
			{
				if (nodeValue.ToLower().Contains("esgn") && nodeValue.StartsWith("5137"))
				{
					//now build the new node value.
					boValue = "1";
					int idx = nodeValue.IndexOf("esgn");
					string value = nodeValue.Substring(idx + 4);
					boValue += value;
				}
				else
				{
					int val = -1;
					if (!Int32.TryParse(nodeValue, out val))
						boValue = "-1";					
				}
			}

			if (nodeName.ToLower() == "refund")
			{
				if (nodeValue.Contains("."))
				{
					double val = Convert.ToDouble(nodeValue);
					int iVal = Convert.ToInt32(val);
					boValue = iVal.ToString();
				}
			}

			base.HandleBackOfficeNode(nodeName, boValue, rawDataFields ,insertCommand);
		}
	}
}

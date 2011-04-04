using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Configuration;

namespace Edge.Services.Reports
{
	static class Config
	{
		internal static IDictionary GetSection(string sectionName)
		{
			IDictionary val = new Dictionary<String, String>();
			try
			{
				val = (IDictionary)(ConfigurationSettings.GetConfig(sectionName));
				return val;
			}
			catch (Exception)
			{
				return null;
			}

		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Configuration;


namespace SchedulerTester
{
    static class Config
    {
        internal static IDictionary GetSection(string sectionName)
        {
            IDictionary val = new Dictionary<String, String>();
            try
            {
              
                val = (IDictionary)(ConfigurationManager.GetSection(sectionName));
                return val;
            }
            catch (Exception)
            {
                return null;
            }

        }
    }
}

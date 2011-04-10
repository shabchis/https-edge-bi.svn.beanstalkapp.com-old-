using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Easynet.Edge.Services.Utilities
{
    static class Config
    {
        internal static IDictionary GetSection(string sectionName) 
        {
            IDictionary val = new Dictionary<String,String>();
            try 
            {
                 val =(IDictionary)(ConfigurationSettings.GetConfig(sectionName));
                 return val;
            }
            catch(Exception)
            {
                return null;
            }
            
        }
    }
}

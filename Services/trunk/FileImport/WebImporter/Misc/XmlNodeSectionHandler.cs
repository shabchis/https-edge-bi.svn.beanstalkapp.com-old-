using System;
using System.Collections.Generic;
using System.Linq;

namespace Easynet.Edge.Services.FileImport.Configuration
{

    public class XmlNodeSectionHandler : System.Configuration.IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            return section;
        }
    }
}
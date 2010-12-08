using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Easynet.Edge.UI.WebPages
{

    public class XmlNodeSectionHandler : System.Configuration.IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            return section;
        }
    }
}
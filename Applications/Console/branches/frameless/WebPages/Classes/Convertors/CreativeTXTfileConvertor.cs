using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Easynet.Edge.Core.Utilities;
using System.Collections.Generic;
using System.IO;

namespace Easynet.Edge.UI.WebPages.Converters
{
    public class CreativeTXTfileConvertor : BaseConvertor
    {
        public CreativeTXTfileConvertor()
            : base()
        {
        }

        public override bool DoWork(string saveFilePath)
        {
            for (int i = 0; i < uploadFilePath.Count; i++)
            {
                File.Copy(uploadFilePath[i], saveFilePath + Path.GetFileName(uploadFilePath[i]), true);
            }
            return true;
        }
    }

}

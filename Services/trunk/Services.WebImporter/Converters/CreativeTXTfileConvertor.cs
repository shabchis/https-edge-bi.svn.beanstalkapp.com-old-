using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using Easynet.Edge.Core.Utilities;
using System.Collections.Generic;
using System.IO;

namespace Easynet.Edge.Services.WebImporter.Converters
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

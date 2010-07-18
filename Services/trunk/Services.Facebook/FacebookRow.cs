using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easynet.Edge.Services.Facebook
{
    public class FacebookRow
    {
        private Dictionary<string, string> _values = new Dictionary<string, string>();

        public Dictionary<string, string> _Values
        {
            get { return _values; }
            set { _values = value; }
        }
    }
}

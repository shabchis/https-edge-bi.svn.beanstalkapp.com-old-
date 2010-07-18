using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.Net;

namespace Easynet.Edge.BusinessObjects
{
    public class Machines : List<Machine>
    {

        public Machines()
        {
        }

        public Machines(string domain)
        {
            DirectoryEntry de = new DirectoryEntry("LDAP://" + domain);
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = "(&(objectClass=computer))";

            SearchResultCollection src = ds.FindAll();

            foreach (SearchResult sr in src)
            {
                Machine m = new Machine(sr);
                Add(m);
            }

        }

    }

    public class Machine
    {

        private string _name = String.Empty;
        private IPAddress _ip = null;


        public Machine()
        {
        }

        public Machine(SearchResult sr)
        {
            if (sr == null)
                throw new ArgumentNullException("Invalid Search Result. Canont be null.");

            if (sr.Properties.Contains("name"))
                _name = sr.Properties["name"][0].ToString();

            try
            {
                IPHostEntry ipEntry = Dns.GetHostEntry(_name);
                IPAddress[] aryLocalAddr = ipEntry.AddressList;

                _ip = aryLocalAddr[0];
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }


        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public IPAddress IP
        {
            get
            {
                return _ip;
            }
            set
            {
                _ip = value;
            }
        }
    }
}

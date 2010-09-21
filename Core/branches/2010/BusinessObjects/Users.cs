using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;

namespace Easynet.Edge.BusinessObjects
{

    public class SystemUsers : List<SystemUser>
    {

        private string _domain;


        public SystemUsers()
        {
        }

        public SystemUsers(string domain)
        {
            _domain = domain;

            Filter();
        }



        public void Filter()
        {
            DirectoryEntry de = new DirectoryEntry("LDAP://" + _domain);
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = "(&(objectClass=user))";

            SearchResultCollection src = ds.FindAll();

            foreach (SearchResult sr in src)
            {
                SystemUser user = new SystemUser(sr);
                Add(user);
            }
        }
    }


    public class SystemUser
    {

        private string _name = String.Empty;
        private string _email = String.Empty;
        private string _cellPhone = String.Empty;
        private string _skype = String.Empty;


        public SystemUser()
        {
        }

        public SystemUser(SearchResult sr)
        {
            if (sr == null)
                throw new ArgumentNullException("Invalid Search Result. Canont be null.");

            if (sr.Properties.Contains("name"))
                _name = sr.Properties["name"][0].ToString();

            if (sr.Properties.Contains("mail"))
                _email = sr.Properties["mail"][0].ToString();

            if (sr.Properties.Contains("mobile"))
                _cellPhone = sr.Properties["mobile"][0].ToString();

            if (sr.Properties.Contains("ipphone"))
                _skype = sr.Properties["ipphone"][0].ToString();
        }


        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string Email
        {
            get
            {
                return _email;
            }
        }

        public string CellPhone
        {
            get
            {
                return _cellPhone;
            }
        }

        public string Skype
        {
            get
            {
                return _skype;
            }
        }

    }
}

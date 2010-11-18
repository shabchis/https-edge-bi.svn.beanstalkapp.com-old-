using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace EdgeBI.Wizards.AccountWizard
{
    class CalculatedMember
    {
        public string Name { get; set; }
        public string Expression { get; set; }
        public string ParentHierarchy { get; set; }
        public bool Visible { get; set; }
        public string Text { get; set; }
        public CalculatedMember()
        {

        }
        public CalculatedMember(string text)
        {
            this.Text = text;
            this.Name = Regex.Match(text, @"(?<=\bCREATE\s*MEMBER\s*.*\[\w*\]?\.\[)[^\]]+", RegexOptions.IgnoreCase | RegexOptions.Multiline).Value.ToUpper();

            if (Regex.Match(text, @"(?<=\bVISIBLE\s*=\s*)[^;]+", RegexOptions.IgnoreCase | RegexOptions.Multiline).Value == "1")
                this.Visible = true;
            else
                this.Visible = false;
            
                
            

        }
        public void ReplaceText(string replaceFrom, string replaceTo)
        {
            string pattern =RegxUtils.CreateExactMatchWholeWordRegExpression (replaceFrom);
            if (!string.IsNullOrEmpty(Name))
                Name = Regex.Replace(Name, pattern, replaceTo, RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(Text))
                Text = Regex.Replace(Text, pattern, replaceTo, RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(Expression))
                Expression = Regex.Replace(Expression, pattern, replaceTo, RegexOptions.IgnoreCase);

        }


    }
    class CalculatedMembersCollection : IDictionary<string, CalculatedMember>
    {

        private const string CalculatedMemberStartOfScript = @"\s*Create\s*member\s*CURRENTCUBE.\[MEASURES\].\[.*\]";
        private Dictionary<string, CalculatedMember> _calculatedMembersDictionary { get; set; }

        public CalculatedMembersCollection()
        {
            _calculatedMembersDictionary = new Dictionary<string, CalculatedMember>();
        }
        public CalculatedMembersCollection(string mdxScriptCommandText)
        {

            _calculatedMembersDictionary = new Dictionary<string, CalculatedMember>();
            string[] calculatedMembersStrings = Regex.Split(mdxScriptCommandText.Trim(), "CREATE", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
            foreach (string str in calculatedMembersStrings)
            {

                if (!string.IsNullOrEmpty(str))
                {
                    string calculatedMemberString = str.Insert(0, " CREATE ");
                    CalculatedMember calculatedMember = new CalculatedMember(calculatedMemberString);
                    _calculatedMembersDictionary.Add(calculatedMember.Name, calculatedMember);
                }



            }

        }
        public void ReplaceCalculatedMembersStrings(string replaceFrom, string replaceTo)
        {
            string pattern = RegxUtils.CreateExactMatchWholeWordRegExpression(replaceFrom);
            string[] keysArray = new string[_calculatedMembersDictionary.Count];
            _calculatedMembersDictionary.Keys.CopyTo(keysArray, 0);
            CalculatedMember calculatedMember;
            foreach (string key in keysArray)
            {
                
                string newKey = Regex.Replace(key, pattern, replaceTo, RegexOptions.IgnoreCase );
                if (newKey.ToUpper() != key.ToUpper())
                {
                    if (_calculatedMembersDictionary.ContainsKey(newKey.ToUpper()))
                        _calculatedMembersDictionary.Remove(key.ToUpper());
                    else
                    {
                         calculatedMember = _calculatedMembersDictionary[key.ToUpper()];
                        _calculatedMembersDictionary.Remove(key.ToUpper());
                        calculatedMember.ReplaceText(replaceFrom, replaceTo);
                        if (!_calculatedMembersDictionary.ContainsKey(calculatedMember.Name.ToUpper())) //should not happend but just in case
                            _calculatedMembersDictionary.Add(calculatedMember.Name.ToUpper(), calculatedMember);

                    }
                }
                else //No problem on the name just the text
                {
                    calculatedMember = _calculatedMembersDictionary[key];  //check if after the replace the dictionary updated
                    calculatedMember.ReplaceText(replaceFrom, replaceTo);

                }



            }







        }
        public string GetText()
        {
            StringBuilder StringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, CalculatedMember> calculatedMember in _calculatedMembersDictionary)
            {
                StringBuilder.AppendLine("\n");
                StringBuilder.AppendLine(calculatedMember.Value.Text);
                StringBuilder.AppendLine("\n");
               


            }
            return StringBuilder.ToString();
        }






        #region IDictionary<string,CalculatedMember> Members

        public void Add(string key, CalculatedMember value)
        {
            _calculatedMembersDictionary.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            if (_calculatedMembersDictionary.ContainsKey(key))
                return true;
            else
                return false;
        }

        public ICollection<string> Keys
        {
            get { return _calculatedMembersDictionary.Keys; }
        }

        public bool Remove(string key)
        {
            return _calculatedMembersDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out CalculatedMember value)
        {
            return _calculatedMembersDictionary.TryGetValue(key, out value);
        }

        public ICollection<CalculatedMember> Values
        {
            get { return _calculatedMembersDictionary.Values; }
        }

        public CalculatedMember this[string key]
        {
            get
            {
                return _calculatedMembersDictionary[key];
            }
            set
            {
                _calculatedMembersDictionary[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,CalculatedMember>> Members

        void ICollection<KeyValuePair<string, CalculatedMember>>.Add(KeyValuePair<string, CalculatedMember> item)
        {
            ((ICollection<KeyValuePair<string, CalculatedMember>>)_calculatedMembersDictionary).Add(item);
        }

        void ICollection<KeyValuePair<string, CalculatedMember>>.Clear()
        {
            ((ICollection<KeyValuePair<string, CalculatedMember>>)_calculatedMembersDictionary).Clear();
        }

        bool ICollection<KeyValuePair<string, CalculatedMember>>.Contains(KeyValuePair<string, CalculatedMember> item)
        {
            return ((ICollection<KeyValuePair<string, CalculatedMember>>)_calculatedMembersDictionary).Contains(item);
        }

        void ICollection<KeyValuePair<string, CalculatedMember>>.CopyTo(KeyValuePair<string, CalculatedMember>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, CalculatedMember>>)_calculatedMembersDictionary).CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<string, CalculatedMember>>.Count
        {
            get { return ((ICollection<KeyValuePair<string, CalculatedMember>>)_calculatedMembersDictionary).Count; }
        }

        bool ICollection<KeyValuePair<string, CalculatedMember>>.IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<string, CalculatedMember>>)_calculatedMembersDictionary).IsReadOnly; }
        }

        bool ICollection<KeyValuePair<string, CalculatedMember>>.Remove(KeyValuePair<string, CalculatedMember> item)
        {
            return ((ICollection<KeyValuePair<string, CalculatedMember>>)_calculatedMembersDictionary).Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,CalculatedMember>> Members

        IEnumerator<KeyValuePair<string, CalculatedMember>> IEnumerable<KeyValuePair<string, CalculatedMember>>.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<string, CalculatedMember>>)_calculatedMembersDictionary).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<string, CalculatedMember>>)_calculatedMembersDictionary).GetEnumerator();
        }

        #endregion
    }
}

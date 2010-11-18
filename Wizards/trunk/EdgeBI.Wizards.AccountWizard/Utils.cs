using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EdgeBI.Wizards.AccountWizard
{
    class RegxUtils
    {
        public static string CreateExactMatchWholeWordRegExpression(string stringToMatch)
        {
           
            StringBuilder pattern = new StringBuilder();
            if (Regex.IsMatch(stringToMatch.Substring(0, 1), "[a-zA-Z0-9]"))// if their is non alphanumeric in the START of the string            
                pattern.Append(String.Format(@"\b{0}", stringToMatch));
            else
                pattern.Append(stringToMatch);

            if (Regex.IsMatch(stringToMatch.Substring(stringToMatch.Length - 1, 1), "[a-zA-Z0-9]"))// if their is non alphanumeric in the END of the string  
                pattern.Append(@"\b");

            return pattern.ToString();
        }
    }
}

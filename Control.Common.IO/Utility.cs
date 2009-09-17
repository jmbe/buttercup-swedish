using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;

namespace Buttercup.Control.Common.IO
{
    public class Utility
    {
        public static string WildcardToRegex(string wildcard)
        {
            if (wildcard == null) return null;

            StringBuilder buffer = new StringBuilder();
            buffer.Append("^");
            char[] chars = wildcard.ToCharArray();
            for (int i = 0; i < chars.Length; ++i)
            {
                if (chars[i] == '*')
                    buffer.Append(".*");
                else if (chars[i] == '?')
                    buffer.Append(".");
                else if ("+()^$.{}[]|\\".IndexOf(chars[i]) != -1)
                    buffer.Append('\\').Append(chars[i]); // prefix all metacharacters with backslash
                else
                    buffer.Append(chars[i]);
            }
            buffer.Append("$");

            return buffer.ToString();
        } 
    }
}

using System.Text;


namespace Buttercup.Control.Common.IO
{
	public class Utility
	{
		#region Methods (1) 

		/// <summary>
		/// Converts the given wildcard to a regex string. Used by In Memory directory implementation.
		/// </summary>
		/// <param name="wildcard">The wildcard expression.</param>
		/// <returns>The wildcard as a regular expression.</returns>
		public static string WildcardToRegex(string wildcard)
		{
			if(wildcard == null)
			{
				return null;
			}

			StringBuilder buffer = new StringBuilder();
			buffer.Append("^");
			char[] chars = wildcard.ToCharArray();
			for(int i = 0; i < chars.Length; ++i)
			{
				if(chars[i] == '*')
				{
					buffer.Append(".*");
				}
				else if(chars[i] == '?')
				{
					buffer.Append(".");
				}
				else if("+()^$.{}[]|\\".IndexOf(chars[i]) != -1)
				{
					buffer.Append('\\').Append(chars[i]); // prefix all metacharacters with backslash
				}
				else
				{
					buffer.Append(chars[i]);
				}
			}
			buffer.Append("$");

			return buffer.ToString();
		}

		#endregion Methods 
	}
}
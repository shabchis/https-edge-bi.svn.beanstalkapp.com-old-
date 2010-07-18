using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Services.DataRetrieval.DataReader;

namespace Easynet.Edge.Services.Google.OrganicRankings
{
	public class Token
	{
		public readonly string[] Start;
		public readonly string End;
		public readonly string Name;

		public Token(string name, string start, string end)
			: this(name, new string[] {start}, end)
		{
		}

		public Token(string name, string[] startList, string end)
		{
			Name = name;
			Start = startList;
			End = end;
		}

	}

	/// <summary>
	/// This class convert EasyForex BackOffice xml row to an BackOffice row.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>15/09/2008</creation_date>
	public class GoogleOrganicRankingsReader: TextDataRowReader<OrganicRankingsRow>
	{
		public GoogleOrganicRankingsReader(string filePath) : base(filePath)
		{
		}

		/// <summary>
		/// Read a BackOffice row from the EasyForex XML file and 
		/// parse the row into currentRow.
		/// </summary>
		/// <returns>current row, null value mean end of file.</returns>
		protected override OrganicRankingsRow GetRow()
		{
			Token[] tokens = new Token[]
			{
				new Token("Url",			"<h2 class=r><a href=\"",					"\""),
				new Token("Title",			new string[] { "class=l", ",'')\">" },		"</a>"),
				new Token("Description",	"class=std>",								"<br>")
			};

			string buffer = String.Empty;
			char[] nextChar = new char[1];
			bool copying = false;
			int tokenIndex = 0;
			int subTokenIndex = 0;
			OrganicRankingsRow row = new OrganicRankingsRow();

			// Keep reading until a complete row is scanned
			while (InternalReader.Read(nextChar, 0, 1) > 0)
			{
				Token token = tokens[tokenIndex];

				// Add the next character to the buffer
				buffer += nextChar[0];

				if (!copying)
				{
					string subToken = token.Start[subTokenIndex];

					// LOOKING FOR THE NEXT VALUE
					if (buffer == subToken)
					{
						if (subTokenIndex == token.Start.Length-1)
						{
							// Found all the start tokens, start copying
							subTokenIndex = 0;
							copying = true;
						}
						else
						{
							// Need to look for the next token, advance to the next
							subTokenIndex++;
						}
						buffer = String.Empty;
					}
					else if (buffer != subToken.Substring(0, buffer.Length))
					{
						// What we have so far does not match the token, try just the current character
						if (nextChar[0] == subToken[0])
						{
							// The last read character matches the beginning of a token, so continue with it
							buffer = nextChar[0].ToString();
						}
						else
						{
							// The current character is worthless as well, so just reset the buffer
							buffer = String.Empty;
						}
					}
				}
				else
				{
					// COPYING A VALUE WHILE LOOKING FOR ITS END TOKEN
					if (buffer.Length >= token.End.Length && buffer.Substring(buffer.Length - token.End.Length, token.End.Length) == token.End)
					{
						copying = false;

						// Apply value (without end token)
						row.ApplyValue(token.Name, Cleanup(buffer.Substring(0, buffer.Length - token.End.Length)));

						if (tokenIndex == tokens.Length -1)
						{
							// Found all tokens
							return row;
						}
						else
						{
							// Move to next token
							tokenIndex++;
							buffer = String.Empty;
						}
					}
				}
			}

			// Arrived to end of file.
			return null;
		}

		/// <summary>
		/// Removes HTML tags and escaped chars ("&quot;" etc.)
		/// </summary>
		string Cleanup(string resultText)
		{
			string output = Regex.Replace(resultText, "<[^>]*>", String.Empty);
			output = HttpUtility.HtmlDecode(output);
			return output;
		}

	}
}

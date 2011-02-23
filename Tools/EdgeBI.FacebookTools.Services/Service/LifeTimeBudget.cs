using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Http;
namespace EdgeBI.FacebookTools.Services.Service
{
	public class LifeTimeBudget
	{
		public FileDescription fileDescription;
		List<JArray> _list = new List<JArray>();
		
		public Stream stream;
		TextWriter t;
		HttpRequestMessage response = new HttpRequestMessage();
		Stack<string> stack = new Stack<string>();
		Dictionary<int, string> colSettings = new Dictionary<int, string>();
		private int _counter = 0;
		public void test()
		{
			
			try
			{
				
				
				_counter = 0;

				

				t = new StreamWriter(this.stream);
				foreach (KeyValuePair<int, ColumnDescriptionAndValues> colDesc in fileDescription.Settings.OrderBy(s => s.Key))
				{
					t.Write("{0}\t", colDesc.Value.ColumnName);
					
				}



				t.WriteLine();
				Dublicate(0); //pay attention yaron should sent the first col as 0 or you will need to change the method
				
				
				
				
				

				
				

			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(System.Net.HttpStatusCode.Forbidden, ex);
			}
			
		}

		//private Stream writeFile()
		//{
		//    //StreamWriter streamWriter = new StreamWriter(@"c:\bulkfile.txt", false, Encoding.Unicode);

		//    //streamWriter.Write(str.ToString());


		//    //return streamWriter.BaseStream;
		//}

		public void Dublicate(int listIndex)
		{
			
			if (listIndex >= fileDescription.Settings.Count)
			{

				int colIndex = 0;
				foreach (string colValue in stack.Reverse())
				{

					t.Write(GetCustomFormat(colIndex, colValue));
					colIndex++;

				}
				_counter++;
				t.WriteLine();
				stack.Pop();
				return;
			}
			else
			{

				List<string> currentCol = (List<string>)fileDescription.Settings[listIndex].values;
				foreach (string val in currentCol)
				{
					stack.Push(val);
					Dublicate(listIndex + 1);
				}
				if (stack.Count > 0)
					stack.Pop();
				return;

			}




		}

		private string GetCustomFormat(int listIndex, string colValue)
		{
			string columnSetting = fileDescription.Settings[listIndex].SettingName;
			string result = string.Empty;
			switch (columnSetting)
			{
				case "Default":
					{
						result = string.Format("{0}\t", colValue);
						break;
					}
				case "Int":
					{
						int temp;
						if (int.TryParse(colValue, out temp))
							result = string.Format("{0}\t", colValue);
						break;
					}
				case "Link":
					{
						Match m = Regex.Match(colValue, @"(?<=\=)[\d]+");
						int nextNum = int.Parse(m.Value);
						result = Regex.Replace(colValue, @"(?<=\=)[\d]+", (nextNum + _counter).ToString());
						result = result + "\t";
						break;
					}
				case "Double":
					{
						double temp;
						if (double.TryParse(colValue, out temp))
							result = string.Format("{0}\t", colValue);
						break;

					}
				case "ad_name":
					{
						string formatNum = (_counter + 1).ToString().PadLeft(3, '0');
						result = string.Format("{0}#{1}\t", colValue, formatNum);
						break;

					}
				default:
					{
						result = string.Format("{0}\t", colValue);
						break;
					}
			}
			return result;
		}






	}
}
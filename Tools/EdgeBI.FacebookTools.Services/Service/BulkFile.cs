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
	public class BulkFile : Istremable
	{
		public FileDescription fileDescription;
		List<JArray> _list = new List<JArray>();
		
		public Stream stream;
		TextWriter textWriter;
		HttpRequestMessage response = new HttpRequestMessage();
		Stack<string> stack = new Stack<string>();
		Dictionary<int, string> colSettings = new Dictionary<int, string>();
		private int _counter = 0;
		public void ProcessStream()
		{			
			try
			{		
				//reset the counter just in case...
				_counter = 0;
				//intitalize the return stream
				textWriter = new StreamWriter(this.stream);

				//witer the titles of the tab delimited file
				foreach (KeyValuePair<int, ColumnDescriptionAndValues> colDesc in fileDescription.Settings.OrderBy(s => s.Key))
				{
					textWriter.Write("{0}\t", colDesc.Value.ColumnName);					
				}
				textWriter.WriteLine();

				//Create the file duplicate the fields create Cartesian product
				CartesianProduct(0); //pay attention yaron should sent the first col as 0 or you will need to change the method
			}
			catch (Exception ex)
			{
				ErrorMessageInterceptor.ThrowError(System.Net.HttpStatusCode.Forbidden, ex);
			}
			
		}


		public void CartesianProduct(int listIndex)
		{
			if (listIndex >= fileDescription.Settings.Count) //the last column then write it on the stream and dont collect on memory
			{
				int colIndex = 0;
				foreach (string colValue in stack.Reverse())
				{

					textWriter.Write(GetCustomFormat(colIndex, colValue)); //write to strem with the correct configuration of every column
					colIndex++;

				}
				_counter++; //this counter is user for specail coonfiguration of some columns "mispar ratz"
				textWriter.WriteLine();
				stack.Pop(); //remove last value 
				return;
			}
			else //still not have a full row not did not pass all columns
			{
				List<string> currentCol = (List<string>)fileDescription.Settings[listIndex].values; //get col by index
				foreach (string val in currentCol) //foreach value we runing the function again (reqursive)
				{
					stack.Push(val); //enter the new value
					CartesianProduct(listIndex + 1);
				}
				if (stack.Count > 0)
					stack.Pop(); //remove the last value
				return; //finish all the values the return
			}
		}
		/// <summary>
		/// Get the custom format by col id
		/// </summary>
		/// <param name="listIndex"></param>
		/// <param name="colValue"></param>
		/// <returns></returns>
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
				case "Counter": //problem in col ad_name should return the nume with '#' before right now not doing it
					{
						int? nextNum = _counter;
						int? fromNum = fileDescription.Settings[listIndex].from;
						if (fromNum != null)
							nextNum += fromNum;
						else
							nextNum += 1;						
						result = Regex.Replace(colValue, @"(\@\@", (nextNum).ToString());
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
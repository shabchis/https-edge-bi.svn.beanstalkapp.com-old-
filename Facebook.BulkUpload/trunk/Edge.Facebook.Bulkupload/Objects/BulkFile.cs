using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace Edge.Facebook.Bulkupload.Objects
{
	
	public class BulkFile
	{
		public FileDescription _fileDescription;
		List<JArray> _list = new List<JArray>();

		public static string Path;
		StreamWriter _streamWriter;
		Stack<string> stack = new Stack<string>();
		Dictionary<int, string> colSettings = new Dictionary<int, string>();
		private int _counter = 0;
		public string CreateFile(FileDescription fileDescription)
		{
			try
			{
				Guid guid = Guid.NewGuid();
				
				_fileDescription = fileDescription;
				//reset the counter just in case...
				_counter = 0;
				//intitalize the return stream
				string path = HttpContext.Current.Server.MapPath("~/Files");
				string fileName=string.Format("BulkUpload{0}.txt", guid.ToString());
				string specificFilePath = System.IO.Path.Combine(path,fileName );
				_streamWriter = new StreamWriter(specificFilePath, false, Encoding.Unicode);

				//witer the titles of the tab delimited file
				foreach (KeyValuePair<int, ColumnDescriptionAndValues> colDesc in _fileDescription.Settings.OrderBy(s => s.Key))
				{
					_streamWriter.Write("{0}\t", colDesc.Value.ColumnName);
				}
				_streamWriter.WriteLine();

				//Create the file duplicate the fields create Cartesian product
				CartesianProduct(0); //pay attention yaron should sent the first col as 0 or you will need to change the method
				_streamWriter.Close();
				string appPath = HttpContext.Current.Request.ApplicationPath;
				if (appPath == "/")
					appPath = string.Empty;
				specificFilePath=string.Format("http://{0}{1}/Files/{2}",HttpContext.Current.Request.ServerVariables["HTTP_HOST"],appPath,fileName);
				//HttpContext.Current.Response.TransmitFile(specificFilePath);
				return specificFilePath;
			}
			finally
			{
				_streamWriter.Close();
				_streamWriter.Dispose();
			}
		}


		public void CartesianProduct(int listIndex)
		{
			if (listIndex >= _fileDescription.Settings.Count) //the last column then write it on the stream and dont collect on memory
			{
				int colIndex = 0;
				foreach (string colValue in stack.Reverse())
				{

					_streamWriter.Write(GetCustomFormat(colIndex, colValue)); //write to strem with the correct configuration of every column
					colIndex++;

				}
				_counter++; //this counter is user for specail coonfiguration of some columns "mispar ratz"
				_streamWriter.WriteLine();
				stack.Pop(); //remove last value 
				return;
			}
			else //still not have a full row not did not pass all columns
			{
				List<string> currentCol = (List<string>)_fileDescription.Settings[listIndex].values; //get col by index
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
			string columnSetting = _fileDescription.Settings[listIndex].SettingName;
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
						else
						{
							if (string.IsNullOrEmpty(colValue))
								result = "\t";
							else
								throw new Exception("Value should be numeric or empty");
						}
						break;
					}
				case "Counter": //problem in col ad_name should return the nume with '#' before right now not doing it
					{
						string finalNumString = string.Empty;
						int pad = 0;
						int? nextNum = _counter;
						int? fromNum = _fileDescription.Settings[listIndex].from;
						if (fromNum != null)
							nextNum += fromNum;
						else
							nextNum += 1;

						if (_fileDescription.Settings[listIndex].PadLeftLength != null && _fileDescription.Settings[listIndex].PadLeftLength > 0)
						{
							pad = (int)_fileDescription.Settings[listIndex].PadLeftLength;
							finalNumString = nextNum.ToString().PadLeft(pad, '0');
						}
						else
							finalNumString = nextNum.ToString();
						result = Regex.Replace(colValue, @"\@\@", (finalNumString).ToString());
						result = result + "\t";
						break;
					}
				case "Double":
					{
						double temp;
						if (double.TryParse(colValue, out temp))
							result = string.Format("{0}\t", colValue);
						else
						{
							if (string.IsNullOrEmpty(colValue))
								result = "\t";
							else
								throw new Exception("Value should be numeric or empty");
						}
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
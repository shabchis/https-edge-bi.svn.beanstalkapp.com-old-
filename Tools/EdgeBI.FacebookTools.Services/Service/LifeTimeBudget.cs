using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Text;
namespace EdgeBI.FacebookTools.Services.Service
{
	public class LifeTimeBudget
	{

		List<JArray> _list = new List<JArray>();
		StringBuilder str = new StringBuilder();
		Stack<string> stack = new Stack<string>();
		Dictionary<int, string> colSettings = new Dictionary<int, string>();
		private int _counter = 0;
		public void test(JObject json)
		{
			try
			{
			
				foreach (JProperty propy in json.Children())
				{
					_list.Add((JArray)propy.Value);
					str.AppendFormat("{0}\t", propy.Name);
					
				}
				str.AppendLine();
				Dublicate(0);
				
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(System.Net.HttpStatusCode.Forbidden, ex);
			}
		}

		public void Dublicate( int listIndex)
		{
			if (listIndex > _list.Count-1)
			{
				_counter++;
				foreach (string col in stack.Reverse())
				{
					str.AppendFormat("{0}\t", col);
					
				}
				str.AppendLine();
				stack.Pop();
				return;
			}
			else
			{
				for (int i = listIndex; i <= _list.Count-1; i++)
				{
					foreach (string val in _list[i].Values())
					{
						stack.Push(val);
						Dublicate(listIndex + 1);						
					}
					if (stack.Count > 0)
						stack.Pop();
					return;
				}
			}
			
		
			
			
		}
	



		

	}
}
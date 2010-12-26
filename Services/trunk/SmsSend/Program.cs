using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.Messaging
{
	class Program
	{
		static int Main(string[] args)
		{
			if (args == null || args.Length < 1)
			{
				Console.WriteLine("Edge SMS Send Utility " + Assembly.GetExecutingAssembly().GetName().Version + "\n");
				Console.WriteLine("smdsend.exe <phone> \"<message>\"");
				Console.WriteLine("\t<phone>:\tPhone number (numbers only) - can put multiple numbers seperated by comma");
				Console.WriteLine("\t<message>:\tMessage to send enclosed in quotes");
				return (int) ExitCode.NotSent;
			}

			if (args.Length < 2)
			{
				Console.WriteLine("Not enough arguments.");
				return (int)ExitCode.NotSent;
			}

			try { SmsMessage.Send(args[1], args[0]); }
			catch (Exception ex)
			{
				Log.Write(SmsMessage.EventLogSource, "SmsMessage.Send failed.", ex);
				return (int)ExitCode.NotSent;
			}

			return (int)ExitCode.Sent;
		}
	}

	enum ExitCode
	{
		NotSent = 0,
		Sent = 1
	}
}

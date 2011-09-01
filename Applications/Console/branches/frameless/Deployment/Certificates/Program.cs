using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;
using System.Text;

namespace Easynet.Edge.UI.Deployment.Prerequisites
{
	static class Program
	{
		delegate bool InstallAction(string[] args);

		[STAThread]
		static int Main(string[] args)
		{
			bool shouldContinue = WaitForApplications();
			if (!shouldContinue)
			{
				Message("Installation canceled.");
				return -1;
			}

			ShowMessages = args.Contains<string>("/msg");
			var toInstall = new InstallAction[] { Certificates, TrustedSites };

			int succeeded = 0;
			foreach (InstallAction action in toInstall)
				if (action(args))
					succeeded++;

			bool success = (succeeded == toInstall.Length);

			// Try this regardless of success;
			WpfPlugin(args);

			Message(success ? "All prerequisites were successfully installed." : "Some prerequisites failed to install. See application event log for details.");

			return success ? 1 : -1;
		}

		#region Messages
		//............................

		static bool ShowMessages;

		static void Message(string text)
		{
			Message(text, EventLogEntryType.Information, null);
		}

		static void Message(string text, EventLogEntryType messageType)
		{
			Message(text, messageType, null);
		}

		static void Message(string text, Exception ex)
		{
			Message(text,  EventLogEntryType.Error, ex);
		}

		static void Message(string text, EventLogEntryType messageType, Exception ex)
		{
			if (String.IsNullOrEmpty(text) && ex != null)
				text = String.Format("{0}: {1}", ex.GetType().Name, ex.Message);

			if (ShowMessages)
				MessageBox.Show(text);

			if (ex != null)
				text += "\n\n" + ex.ToString();

			try { EventLog.WriteEntry("Edge.BI Installation", text, messageType); }
			catch { }
		}

		//............................
		#endregion

		#region WaitForApplications
		//............................

		static Dictionary<string,string> _waitApps = new Dictionary<string,string>()
		{
			{ "firefox", "Mozilla Firefox" },
			{ "chrome", "Google Chrome" },
			{ "Safari", "Safari" },
			{ "opera", "Opera" }
		};

		//static string processNames = 
		static bool WaitForApplications()
		{
			DialogResult result = DialogResult.Retry;
			List<string> running = new List<string>();

			while (result == DialogResult.Retry)
			{
				running.Clear();

				foreach (Process process in Process.GetProcesses())
				{
					string desc;
					if (_waitApps.TryGetValue(process.ProcessName, out desc) && !running.Contains(desc))
						running.Add(desc);
				}

				if (running.Count > 0)
				{
					StringBuilder builder = new StringBuilder();
					foreach (string desc in running)
						builder.AppendLine(desc);

					result = MessageBox.Show("Please close the following applications before continuing:\n\n" + builder.ToString(),
						"Waiting",
						MessageBoxButtons.RetryCancel,
						MessageBoxIcon.Exclamation);
				}
				else
					result = DialogResult.OK;
			}

			return result == DialogResult.OK;
		}

		//............................
		#endregion

		#region InstallAction: Certificates
		//............................

		public static bool Certificates(string[] args)
		{
			const bool installToRoot = true;
			bool checkOnly = args.Contains<string>("/check");
			Stream stream = Assembly.GetEntryAssembly().GetManifestResourceStream("Easynet.Edge.UI.Deployment.Prerequisites.Certificate-public.cer");
			byte[] certificateData = new byte[stream.Length];
			stream.Read(certificateData, 0, Convert.ToInt32(stream.Length));

			try
			{
				X509Certificate2 cert = new X509Certificate2(certificateData);
				X509Store[] stores = new X509Store[installToRoot ? 2 : 1];
				stores[0] = new X509Store(StoreName.TrustedPublisher, StoreLocation.CurrentUser);
				if (installToRoot)
					stores[1] = new X509Store(StoreName.Root, StoreLocation.CurrentUser);

				Exception exception = null;
				bool missing = false;
				foreach (X509Store store in stores)
				{
					store.Open(OpenFlags.ReadWrite);

					if (store.Certificates.Contains(cert))
						continue;

					missing = true;

					if (!checkOnly)
					{
						try { store.Add(cert); }
						catch (Exception ex)
						{
							exception = ex;
						}
					}

					store.Close();

					if (exception != null)
						break;
				}

				bool error = exception != null;

				Message(
					error ?
						"Error while installing certificates." :
					missing ?
						"Missing certificates were successfully installed."
					:
						"All certificates are already installed, no action taken.",
					exception
				);

				return !error;
			}
			catch (Exception ex)
			{
				Message(null, ex); ;
				return false;
			}
		}

		//............................
		#endregion

		#region InstallAction: Trusted Sites
		//............................

		public static bool TrustedSites(string[] args)
		{
			// Based on http://stackoverflow.com/questions/972345/programmatically-add-trusted-sites-to-internet-explorer
			const string domainsKeyLocation = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains";
			const string domain = @"edge-bi.com";
			const int trustedSiteZone = 0x2;

			try
			{
				RegistryKey domainKey = Registry.CurrentUser.GetOrCreateSubKey(domainsKeyLocation, domain, true);

				object wildcard = domainKey.GetValue("*");
				if (wildcard == null || Convert.ToInt32(wildcard) != trustedSiteZone)
				{
					domainKey.SetValue("*", trustedSiteZone, RegistryValueKind.DWord);
					Message(String.Format("Configured trusted site status for {0} in {1}.", domain, domainKey.ToString()));
				}
				else
					Message("Trusted sites are already defined.");

				return true;
			}
			catch (Exception ex)
			{
				Message(null, ex);
				return false;
			}
		}

		public static RegistryKey GetOrCreateSubKey(this RegistryKey registryKey, string parentKeyLocation, string key, bool writable)
		{
			string keyLocation = string.Format(@"{0}\{1}", parentKeyLocation, key);
			RegistryKey foundRegistryKey = registryKey.OpenSubKey(keyLocation, writable);
			return foundRegistryKey ?? registryKey.CreateSubKey(parentKeyLocation, key);
		}

		public static RegistryKey CreateSubKey(this RegistryKey registryKey, string parentKeyLocation, string key)
		{
			RegistryKey parentKey = registryKey.OpenSubKey(parentKeyLocation, true); //must be writable == true
			if (parentKey == null) { throw new NullReferenceException(string.Format("Missing parent key: {0}", parentKeyLocation)); }

			RegistryKey createdKey = parentKey.CreateSubKey(key);
			if (createdKey == null) { throw new Exception(string.Format("Key not created: {0}", key)); }

			return createdKey;
		}

		//............................
		#endregion

		#region InstallAction: WPF Mozilla Plugin
		//............................

		public static bool WpfPlugin(string[] args)
		{
			try
			{
				// =============================
				// STEP 1 - Registry

				//HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node
				RegistryKey softwareKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node");
				if (softwareKey == null)
					softwareKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");

				if (softwareKey == null)
				{
					Message(@"Could not open HKEY_LOCAL_MACHINE\SOFTWARE registry key.");
					return false;
				}

				const string key_mozillaorg = @"mozilla.org\Mozilla";
				RegistryKey mozillaOrgKey = softwareKey.OpenSubKey(key_mozillaorg);
				if (mozillaOrgKey == null)
				{
					Message(String.Format("{0}\\{1} not found.", softwareKey.ToString(), key_mozillaorg), EventLogEntryType.Error);
					return false;
				}

				const string val_CurrentVersionNum = "CurrentVersion";
				string currentVersionNum = mozillaOrgKey.GetValue(val_CurrentVersionNum) as string;
				if (currentVersionNum == null)
				{
					Message(String.Format("{0}[{1}] not found.", mozillaOrgKey.ToString(), val_CurrentVersionNum), EventLogEntryType.Error);
					return false;
				}

				string key_Extensions = String.Format(@"Mozilla\Mozilla Firefox {0}\extensions", currentVersionNum);
				RegistryKey extensionsKey = softwareKey.OpenSubKey(key_Extensions);
				if (extensionsKey == null)
				{
					Message(String.Format("{0}\\{1} not found.", softwareKey.ToString(), key_Extensions), EventLogEntryType.Error);
					return false;
				}

				const string val_PluginsDir = "Plugins";
				string pluginsDir = extensionsKey.GetValue(val_PluginsDir) as string;
				if (pluginsDir == null)
				{
					Message(String.Format("{0}[{1}] not found.", extensionsKey.ToString(), val_PluginsDir), EventLogEntryType.Error);
					return false;
				}

				// =============================
				// STEP 2 - Copy to Directory

				const string pluginFile = "NPWPF.dll";
				string pluginDestination = Path.Combine(pluginsDir, pluginFile);
				if (File.Exists(pluginDestination))
				{
					Message("Firefox WPF plugin is already installed.");
				}
				else
				{
					using (Stream pluginRead = Assembly.GetEntryAssembly().GetManifestResourceStream("Easynet.Edge.UI.Deployment.Prerequisites." + pluginFile))
					{
						using (FileStream pluginWrite = File.Open(pluginDestination, FileMode.CreateNew))
						{
							byte[] buffer = new byte[256];
							int count;
							while ((count = pluginRead.Read(buffer, 0, buffer.Length)) > 0)
								pluginWrite.Write(buffer, 0, count);
						}
					}

					Message("Firefox WPF plugin successfully installed.");
				}

				// =============================
				// STEP 3 - Environment path

				try
				{
					Environment.SetEnvironmentVariable(
						"PATH",
						Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) + ";" + Path.GetDirectoryName(pluginsDir),
						EnvironmentVariableTarget.Machine
						);
				}
				catch (Exception ex)
				{
					Message("Failed to set PATH environment variable for cross-browser XBAP compatibility.", ex);
					return false;
				}

				// =============================
				// DONE

				return true;
			}
			catch (Exception ex)
			{
				Message(null, ex);
				return false;
			}

		}

		//............................
		#endregion
	}
}

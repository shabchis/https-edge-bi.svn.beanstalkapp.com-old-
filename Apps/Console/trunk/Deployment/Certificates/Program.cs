using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;

namespace Easynet.Edge.UI.Deployment.Certificates
{
	static class Program
	{
		[STAThread]
		static int Main(string[] args)
		{
			const bool installToRoot = true;
			bool checkOnly = args.Contains<string>("/check");
			bool msgBoxes = args.Contains<string>("/msg");

			Stream stream = Assembly.GetEntryAssembly().GetManifestResourceStream("Easynet.Edge.UI.Deployment.Certificates.Certificate-public.cer");
			byte[] certificateData = new byte[stream.Length];
			stream.Read(certificateData, 0, Convert.ToInt32(stream.Length));

			try
			{
				X509Certificate2 cert = new X509Certificate2(certificateData);
				X509Store[] stores = new X509Store[installToRoot ? 2 : 1];
				stores[0] = new X509Store(StoreName.TrustedPublisher, StoreLocation.CurrentUser);
				if (installToRoot)
					stores[1] = new X509Store(StoreName.Root, StoreLocation.CurrentUser);

				bool error = false;
				bool missing = false;
				foreach (X509Store store in stores)
				{
					store.Open(OpenFlags.ReadWrite);

					bool exists = store.Certificates.Contains(cert);
					missing = missing || !exists;

					if (!exists && !checkOnly)
					{
						try { store.Add(cert); }
						catch
						{
							error = true;
						}
					}

					store.Close();

					if (error)
						break;
				}

				if (error)
				{
					if (msgBoxes) MessageBox.Show("Error while installing certificates.");
					return -1;
				}

				if (missing)
				{
					if (msgBoxes) MessageBox.Show("Certificates will be installed.");
					return 0;
				}
				else
				{
					if (msgBoxes) MessageBox.Show("Certificates were already installed.");
					return 1;
				}
			}
			catch (Exception ex)
			{
				if (msgBoxes) MessageBox.Show(ex.ToString());
				return -1;
			}
		}
	}
}

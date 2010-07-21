using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core;
using Eggplant.Persistence;

namespace Edge.Data.Pipeline
{
	public class ExampleRetrieverService : Service
	{
		protected override ServiceOutcome DoWork()
		{
			// Create a list
			Delivery delivery = new Delivery();
			
			// Add files
			delivery.Files.Add(new DeliveryFile
			{
				Url = "http://wwww.chunk.com?file=1",
				ReaderType = typeof(GoogleAdwordsReportReader),
				ReaderParams = new Settings()
				{
					{"FilterCampaigns", new string[] {"Georgie", "Boy"}},
					{"UseAccoutHeader", true}
				}
			});
			delivery.Files.Add(new DeliveryFile("http://wwww.chunk.com?file=2", typeof(GoogleAdwordsReportReader)));
			
			// Download and report download progress
			delivery.Download(p => ReportProgress(p));

			Instance.ParentInstance.State["TargetDelivery"] = delivery;

			return ServiceOutcome.Success;
		}
	}

	public class ExampleProcessorService : Service
	{
		protected override ServiceOutcome DoWork()
		{
			Delivery delivery = Delivery.Get(Instance.ParentInstance);

			foreach (DeliveryFile file in delivery.Files)
			{
				using (PersistenceProvider.Open("otlp"))
				{
					using (DeliveryFileReader<PpcRow> reader = file.OpenReader())
					{
						while (reader.Read())
						{

						}
					}
				}
			}
		}
	}

	public class Delivery
	{
		public int ID;
		public List<DeliveryFile> Files;
		public Dictionary<string, string> Parameters;
	}

	public class DeliveryFile
	{
		public int ID;
		public int DeliveryID;
		public int RetrieverInstanceID;
		public int ProcessorInstanceID;
		public Type ReaderType;
		public Dictionary<string, string> Parameters;
	}
}

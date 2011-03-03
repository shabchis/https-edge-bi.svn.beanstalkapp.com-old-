using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using System.ServiceModel;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data.Proxy;
using Easynet.Edge.Core.Workflow;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core;
using EdgeBI.Data.Pipeline;
using Services.Data.Pipeline;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Reflection;
using EdgeBI.Data.Pipeline.Objects;
using GotDotNet.XPath;


namespace Easynet.Edge.Services.Bing
{
    public class BingProcessor : Service
    {

        protected override ServiceOutcome DoWork()
        {
            Delivery delivery = new Delivery();
            int deliveryId;
            try
            {
                deliveryId = Int32.Parse(Instance.Configuration.Options["DeliveryID"]);
            }
            catch
            {
                deliveryId = delivery.GetDeliveryIdByInstance(Instance.ParentInstance.InstanceID);
            }
            delivery.GetDelivery(deliveryId);
            ProcessData(delivery);
            return ServiceOutcome.Success;
        }

       private void ProcessData(Delivery delivery)
        {
            Type objType = null;
            switch (Instance.Configuration.Options["ReportReaderType"])
            {
                case "BingKeywordReportReader":
                    objType = typeof(BingKeywordReportReader);
                    break;
                case "BingAdPerformanceReportReader":
                    objType = typeof(BingAdPerformanceReportReader);
                    break;
            }
            string[] filesPath = new string[2];
            for(int i=0;delivery.Files.Count >= i+1;i++) //  DeliveryFile df in delivery.Files)
            {
                Type dRederType = Type.GetType(delivery.Files[i].ReaderType);
                if (dRederType == typeof(BingAdPerformanceReportReader))
                    filesPath[1] = delivery.Files[i].FilePath;
                else if (dRederType == typeof(BingKeywordReportReader))
                    filesPath[0] = delivery.Files[i].FilePath;    
            }
            //BingKeywordReportReader bingReader = new BingKeywordReportReader(df.FilePath);
            //לבדוק שזה בינגdf.ReaderType

            using (RowReader<PpcDataUnit> reader = (RowReader<PpcDataUnit>)Activator.CreateInstance(objType, new object[]{filesPath}))
            {
                using (DataManager.Current.OpenConnection())
                {
                    DataManager.Current.StartTransaction();

                    while (reader.Read())
                    {
                        if(reader.CurrentRow.AccountID > 0 )
                            reader.CurrentRow.Save();
                    }
                    DataManager.Current.CommitTransaction();
                }
            }
        }
    }
}

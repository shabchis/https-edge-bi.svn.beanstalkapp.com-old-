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

            foreach (DeliveryFile df in delivery.Files)
            {
                BingKeywordReportReader bingReader = new BingKeywordReportReader(df.FilePath);

                using (RowReader<PpcDataUnit> reader = (RowReader<PpcDataUnit>)Activator.CreateInstance(df.ReaderType, df.FilePath))
                {
                    using (DataManager.Current.OpenConnection())
                    {
                        DataManager.Current.StartTransaction();

                        while (reader.Read())
                        {
                            reader.CurrentRow.Save();
                        }

                        DataManager.Current.CommitTransaction();
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;

namespace Easynet.Edge.Services.DemoService
{
    public class DemoService: Service
    {
        protected override ServiceOutcome DoWork()
        {   
            Console.WriteLine("------------------------------");
            Console.WriteLine("AccountID: {0}", Instance.AccountID);
            Console.WriteLine("TestMode (option): {0}", Instance.Configuration.Options["TestMode"]);
            Console.WriteLine("------------------------------");

            return ServiceOutcome.Success;
        }
    }
}

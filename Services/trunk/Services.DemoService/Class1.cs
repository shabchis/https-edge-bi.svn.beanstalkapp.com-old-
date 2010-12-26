using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;

namespace $safeprojectname$
{
    public class DemoService: Service
    {
		protected override void OnInit()
		{
			// TODO: Use OnInit to set up anything that needs Instance or Instance.Configuration
			// (Instance is not available in the constructor)
		}

        protected override ServiceOutcome DoWork()
        {   
			// TODO: perform the service logic here. Use ReportProgress() to report your progress.

			// Example: display service metadata on console
            Console.WriteLine("------------------------------");
			Console.WriteLine("AccountID: {0}", Instance.AccountID);
			Console.WriteLine("Service name: {0}", Instance.Configuration.Name);
            Console.WriteLine("Options: {0}", Instance.Configuration.Options.ToString());
            Console.WriteLine("------------------------------");

			// ****************************
			// IMPORTANT: use try..catch only for exceptions you know how to handle!
			// DO NOT JUST CATCH AND IGNORE EXCEPTIONS
			// ****************************

			// TODO: return the correct status.
			// If you return Unspecified, the service will not quit and wait for Service.Run()
			// to be called again by another service.
            return ServiceOutcome.Success;
        }

		protected override void OnEnded(ServiceOutcome outcome)
		{
			// TODO: you can perform different operations here based on the 'outcome' variable.
			// Avoid doing complicated operations here that can take a long time or can cause exceptions
			// to be thrown.
			// If you don't need to do anything here, just delete this entire method.
		}
    }
}

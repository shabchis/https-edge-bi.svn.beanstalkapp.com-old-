﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Services.Bing.ReportingService;
using System.Xml.Serialization;
using System.ServiceModel;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data.Proxy;
using Easynet.Edge.Core.Workflow;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core;
using Easynet.Edge.Services.DataRetrieval.Retriever;
using System.Net;
using System.IO;


namespace Services.Bing
{
    public class BingRetriever : BaseRetriever
    {
        string BingFileName;
        
        protected override ServiceOutcome DoWork()
        {
            try
            {
                BingRetrieverData();
                if (BingFileName.Length > 0)
                {
                    CreateDelivery();
                    return ServiceOutcome.Success;

                }
                else
                    throw new Exception("No file to deliver");


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void CreateDelivery()
        {
            // Create a new delivery with a description
            Delivery delivery = new Delivery();

            // Assign this delivery to an account/client/scope
            delivery.AccountID = Instance.AccountID;

            // Get the target datetime from the configuration
            delivery.DateCreated = DateTime.Parse(Instance.Configuration.Options["DateTime"]).ToLocalTime();

            // Add files
            delivery.Files.Add(new DeliveryFile
            {
                FilePath = BingFileName,
                ReaderType = typeof(BingRetriever)});
        }

        private void BingRetrieverData()
        {
            //Init parameters from configuration
            int columns = Convert.ToInt32(Instance.Configuration.Options["Report_Num_Columns"]);
            int[] accountIds = { Convert.ToInt32(Instance.Configuration.Options["Report_Num_Columns"]) };
            string appToken = Instance.Configuration.Options["appToken"];
            string devToken = Instance.Configuration.Options["devToken"];
            string username = Instance.Configuration.Options["username"];
            string password = Instance.Configuration.Options["password"];
            string customerid = Instance.Configuration.Options["customerid"];
            string customeraccountid = Instance.Configuration.Options["customeraccountid"];
            string zipFileName = Instance.Configuration.Options["zipFileName"];

            ReportingService.AdPerformanceReportRequest request = new ReportingService.AdPerformanceReportRequest();
            // Specify the language for the report.
            request.Language = ReportingService.ReportLanguage.English;
            // Specify the format of the report.
            request.Format = ReportingService.ReportFormat.Xml;
            request.ReturnOnlyCompleteData = false;
            
            // Specify the columns that will be in the report.
            request.Columns = new ReportingService.AdPerformanceReportColumn[columns];
            for (int i = 0 ; i < columns; i++)
                request.Columns[i] = (ReportingService.AdPerformanceReportColumn)Enum.Parse(typeof(ReportingService.AdPerformanceReportColumn), "ReportingService.AdPerformanceReportColumn.AccountName", true);

            // Specify the scope of the report. This example goes down 
            // only to the account level, but you can request a report for any 
            // number of accounts, ad groups, and campaigns.
            request.Scope = new ReportingService.AccountThroughAdGroupReportScope();
            request.Scope.AccountIds = accountIds;
            request.Scope.AdGroups = null;
            request.Scope.Campaigns = null;

            // Specify the filter for the report. This example requests 
            // only search ads that were displayed in the United States to be in 
            // the report.
            request.Filter = new ReportingService.AdPerformanceReportFilter();
            request.Filter.AdDistribution = ReportingService.AdDistributionReportFilter.Search;
            request.Filter.LanguageAndRegion = ReportingService.LanguageAndRegionReportFilter.UnitedStates;

            // Create and initialize the ReportingServiceClient object.
            ReportingService.ReportingServiceClient service = new ReportingService.ReportingServiceClient();

            // Submit the report request.
            try
            {
                // Create and initialize the QueueReportRequest object.
                ReportingService.SubmitGenerateReportRequest submitRequest = new ReportingService.SubmitGenerateReportRequest();
                submitRequest.ApplicationToken = appToken;
                submitRequest.DeveloperToken = devToken;
                submitRequest.UserName = username;
                submitRequest.Password = password;
                submitRequest.CustomerId = customerid;//"770585";
                submitRequest.CustomerAccountId = customeraccountid;// "770585";
                submitRequest.ReportRequest = request;

                // Set the start date for the report to one month before today.
                DateTime startDate = DateTime.Today.AddDays(-1);
                request.Time = new ReportTime();
                request.Time.CustomDateRangeStart = new Date();
                request.Time.CustomDateRangeStart.Day = startDate.Day;
                request.Time.CustomDateRangeStart.Month = startDate.Month;
                request.Time.CustomDateRangeStart.Year = startDate.Year;

                // Set the end date to today.
                DateTime endDate = DateTime.Today;
                request.Time.CustomDateRangeEnd = new Date();
                request.Time.CustomDateRangeEnd.Day = endDate.Day;
                request.Time.CustomDateRangeEnd.Month = endDate.Month;
                request.Time.CustomDateRangeEnd.Year = endDate.Year;


                // Submit the report request. This will throw an exception if 
                // an error occurs.
                ReportingService.SubmitGenerateReportResponse queueResponse;
                queueResponse = service.SubmitGenerateReport(submitRequest);

                // Poll to get the status of the report until it is complete.
                int waitMinutes = 1;
                int maxWaitMinutes = 120;
                DateTime startTime = DateTime.Now;
                int elapsedMinutes = 0;

                // Initialize the GetReportStatusResponse object to an error in 
                // case an error occurs. The error will be handled below.
                ReportingService.PollGenerateReportResponse pollResponse = null;
                do
                {
                    // Wait the specified number of minutes before polling.
                    System.Threading.Thread.Sleep(waitMinutes * 60 * 1000);
                    elapsedMinutes = DateTime.Now.Subtract(startTime).Minutes;

                    // Get the status of the report.
                    pollResponse = CheckReportStatus(username, password, appToken, devToken, customerid, service, queueResponse.ReportRequestId);

                    if (ReportingService.ReportRequestStatusType.Success == pollResponse.ReportRequestStatus.Status)
                    {
                        // The report is ready.
                        break;
                    }
                    else if (ReportingService.ReportRequestStatusType.Pending ==
                        pollResponse.ReportRequestStatus.Status)
                    {
                        // The report is not ready yet.
                        continue;
                    }
                    else
                    {
                        // An error occurred.
                        break;
                    }
                } while (elapsedMinutes < maxWaitMinutes);

                // If the report was created, download it.
                if ((null != pollResponse) &&
                    (ReportingService.ReportRequestStatusType.Success ==
                    pollResponse.ReportRequestStatus.Status))
                {
                    // Download the file.
                    DownloadReport(
                        pollResponse.ReportRequestStatus.ReportDownloadUrl,
                        zipFileName);
                }
            }

            catch (FaultException<AdApiFaultDetail> fault)
            {
                ReportingService.AdApiFaultDetail faultDetail = fault.Detail;

                // Write the API errors.
                foreach (ReportingService.AdApiError opError in faultDetail.Errors)
                {
                    Console.WriteLine(
                        String.Format("Error {0}:", opError.Code));
                    Console.WriteLine(
                        String.Format("\tMessage: \"{0}\"", opError.Message));
                }

            }

            catch (FaultException<ApiFaultDetail> fault)
            {
                ReportingService.ApiFaultDetail faultDetail = fault.Detail;

                // Display service operation error information.
                foreach (ReportingService.OperationError opError in faultDetail.OperationErrors)
                {
                    Console.Write("Operation error");
                    Console.WriteLine(" '{0}' ({1}) encountered.",
                                      opError.Message,
                                      opError.Code);
                }

                // Display batch error information.
                foreach (ReportingService.BatchError batchError in faultDetail.BatchErrors)
                {
                    Console.Write("Batch error");
                    Console.Write(" '{0}' ({1}) encountered",
                                   batchError.Message,
                                   batchError.ErrorCode);
                }

            }

            // Capture exceptions on the client that are unrelated to
            // the adCenter API. An example would be an 
            // out-of-memory condition on the client.
            catch (Exception e)
            {
                Console.WriteLine("Error '{0}' encountered.",
                    e.Message);
            }
            finally
            {
                // To be sure you close the service.
                service.Close();
                if(File.Exists(zipFileName))
                    BingFileName = zipFileName;
            }
        }

        public static ReportingService.PollGenerateReportResponse CheckReportStatus(string username, string password, string appToken, string devToken, string customerId, ReportingService.ReportingServiceClient service, string reportId)
        {
            ReportingService.PollGenerateReportRequest pollRequest = new ReportingService.PollGenerateReportRequest();
            pollRequest.ApplicationToken = null;
            pollRequest.DeveloperToken = devToken;
            pollRequest.UserName = username;
            pollRequest.Password = password;
            pollRequest.ReportRequestId = reportId;

            // Get the status of the report.
            return service.PollGenerateReport(pollRequest);
        }

        public static void DownloadReport(string downloadUrl, string zipFileName)
        {
            // Open a connection to the URL where the report is available.
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(downloadUrl);
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            Stream httpStream = response.GetResponseStream();

            // Open the report file.
            FileInfo zipFileInfo = new FileInfo(zipFileName);
            if (!zipFileInfo.Directory.Exists)
            {
                zipFileInfo.Directory.Create();
            }
            FileStream fileStream = new FileStream(
                zipFileInfo.FullName,
                FileMode.Create);
            BinaryWriter binaryWriter = new BinaryWriter(fileStream);
            BinaryReader binaryReader = new BinaryReader(httpStream);
            try
            {
                // Read the report and save it to the file.
                int bufferSize = 100000;
                while (true)
                {
                    // Read report data from API.
                    byte[] buffer = binaryReader.ReadBytes(bufferSize);

                    // Write report data to file.
                    binaryWriter.Write(buffer);

                    // If the end of the report is reached, break out of the 
                    // loop.
                    if (buffer.Length != bufferSize)
                    {
                        break;
                    }
                }
            }
            finally
            {
                // Clean up.
                binaryWriter.Close();
                binaryReader.Close();
                fileStream.Close();
                httpStream.Close();
            }
        }
    }
}
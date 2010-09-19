using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Services.CurrencySampler.CurrenciesService;
using System.Xml.Serialization;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using System.Data;

namespace Easynet.Edge.Services.CurrencySampler
{
	public class CurrencySamplerService : Service
	{
		protected override ServiceOutcome DoWork()
		{


			#region Get Service Params
			XigniteCurrencies exchangeRates;
			exchangeRates = new XigniteCurrencies();
			Header header = new Header();
			header.Username = Instance.Configuration.Options["UserName"];
			header.Password = Instance.Configuration.Options["Password"];
			exchangeRates.HeaderValue = header;


			string fromCurrency = Instance.Configuration.Options["FromCurrency"];
			StringBuilder toCurrency = new StringBuilder();
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand cmd = DataManager.CreateCommand(@"SELECT ID,CurrencyIso
												FROM Currencies");
				Dictionary<string, int> currencyDictionary = new Dictionary<string, int>();
				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					currencyDictionary.Add(reader.GetString(1), reader.GetInt32(0));
					toCurrency.Append(reader.GetString(1));
					toCurrency.Append(",");

				}
				reader.Close();
				toCurrency.Remove(toCurrency.Length - 1, 1); // remove last "," 

			#endregion



				////Only for tests without the real service
		  //      CrossRate[] rates = new CrossRate[2];
		  //      CrossRate rt = new CrossRate() { To = new Currency() { Name = "EUR" }, Rate = 1.3 };
		  //      rates[0] = rt;
		  //      rt = new CrossRate() { To = new Currency() { Name = "EUS" }, Rate = 2.3 };
		  //      rates[1] = rt;
				 
	



				//the service
				CrossRate[] rates = exchangeRates.GetLatestCrossRates(fromCurrency, toCurrency.ToString());


				if (rates != null)
				{
					if (rates[0].Outcome != OutcomeTypes.Success)
					{
						throw new Exception(string.Format("Web service error:{0}", rates[0].Message));
					}
					else
					{

						
						foreach (CrossRate rate in rates)
						{					
								cmd = DataManager.CreateCommand(@"INSERT INTO [testdb].[dbo].[ExchangeRates]
																   ([RateDateTime]
																   ,[currencyID]
																   ,[Rate]
																	,[DateValue])
																	VALUES (
																	@rateDateTime:DateTime,
																	@id:int,																	
																	@rate:Decimal,
																	@dateValue:int)");
								cmd.Parameters["@rateDateTime"].Value = DateTime.Now;
								cmd.Parameters["@id"].Value = currencyDictionary[rate.To.Symbol.ToString()];
								cmd.Parameters["@rate"].Value = rate.Rate;
								cmd.Parameters["@dateValue"].Value = Core.Utilities.DayCode.ToDayCode(DateTime.Today);
								cmd.ExecuteNonQuery();
								
						}
					}
				}

			}









			return ServiceOutcome.Success;
		}
	}
}

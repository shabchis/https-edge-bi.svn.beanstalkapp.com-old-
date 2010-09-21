using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Easynet.Edge.Core.Utilities
{
	public class ScheduleConvertor
	{
		static ScheduleConvertor()
		{
		}


		private static bool TryGetDate(string date, out DateTime datetime)
		{
			datetime = DateTime.Now;
			int firstIndex = date.IndexOf(':');

			if (firstIndex < 0)
			{
				Log.Write(string.Format("No calendar unit in date {0}.", date), LogMessageType.Warning);
				return false;
			}

			char calendarUnit = date[firstIndex - 1];
			date = date.Substring(firstIndex + 1);
			int dateInterval = 0;

			if (!Int32.TryParse(date, out dateInterval))
			{
				Log.Write(string.Format("can't convert date interval {0} to int.", date), LogMessageType.Warning);
				return false;
			}

			// Return date acroding to the interval date in the string.
			if (calendarUnit.ToString().ToUpper() == "M")
				datetime = DateTime.Now.AddMonths(dateInterval);
			else if (calendarUnit.ToString().ToUpper() == "W")
				datetime = DateTime.Now.AddDays(7 * dateInterval);
			else if (calendarUnit.ToString().ToUpper() == "D")
				datetime = DateTime.Now.AddDays(dateInterval);
			else
				return false;

			return true;
		}

		public static ArrayList GetRangeDate(string from, string to, string exactUnits)
		{
			ArrayList dates = new ArrayList();
			int startDay;
			int endDay;
			bool hasExactUnits = GetExactUnits(exactUnits, out startDay, out endDay);

			if ((from == null) || (from == string.Empty) || (to == null) || (to == string.Empty))
				return null;

			DateTime fromDate; 
			DateTime toDate;

			if (!TryGetDate(from, out fromDate) || !TryGetDate(to, out toDate))
				return null;

			if (fromDate > toDate)
			{
				Log.Write(string.Format("From date {0} isn't lower from to date {1}.", fromDate.ToShortDateString(), toDate.ToShortDateString()), LogMessageType.Error);
				return null;
			}
						
			// ExactUnits are used only for months.
			if (hasExactUnits)
			{
				while (fromDate.Date <= toDate.Date)
				{
					DateTime date = new DateTime(fromDate.Year, fromDate.Month, startDay);
					for (int i = startDay; i <= endDay; ++i)
					{
						if ((date.Month > fromDate.Month) || (date.Date >= DateTime.Now.Date))
							break;
						dates.Add(date);
						date = date.AddDays(1);
						
					}

					fromDate = fromDate.AddMonths(1);
				}
			}
			else
			{
				// We never run a rerun service on today.
				if (toDate.Date == DateTime.Now.Date)
					toDate.AddDays(-1);

				if (fromDate == toDate)
					dates.Add(fromDate);

				while (fromDate.Date <= toDate.Date)
				{
					dates.Add(fromDate);
					fromDate = fromDate.AddDays(1);
				}
			}

			return dates;
		}

		private static bool GetExactUnits(string exactUnits, out int startDay, out int endDay)
		{
			startDay = 0;
			endDay = 0;

			if (exactUnits == string.Empty)
				return false;

			int commaIndex = exactUnits.IndexOf('-');

			// Handle only one value in the exactUnits.
			if (commaIndex < 0)
			{
				if (!Int32.TryParse(exactUnits, out startDay))
					return ErrorConvertExactUnits(exactUnits);
				endDay = startDay;
				return true;
			}

			if (!Int32.TryParse(exactUnits.Substring(0,commaIndex) , out startDay))
				return ErrorConvertExactUnits(exactUnits);

			if (!Int32.TryParse(exactUnits.Substring(commaIndex + 1), out endDay))
				return ErrorConvertExactUnits(exactUnits);

			if (startDay > endDay || startDay < 1 || endDay < 1 || startDay > 31 || endDay > 31)
				return ErrorConvertExactUnits(exactUnits);	

			return true;
		}

		private static bool ErrorConvertExactUnits(string exactUnits)
		{
			Log.Write(string.Format("can't convert exactUnits - {0}", exactUnits), LogMessageType.Error);
			return false;
		}
		
		public static bool CheckFullSchedule(string schedule)
		{
			// Handle freuency
			if (schedule[0].ToString().ToUpper() == "F")
			{
				// For future use.
			}
			else
			{
				int firstIndex = schedule.IndexOf(':');

				if (firstIndex < 0)
				{
					// Run the service every day (D without a number)
					if (schedule.ToUpper() == "D")
						return true;
					else
					{
						Log.Write(string.Format("No calendar unit in {0}."), schedule, LogMessageType.Warning);
						return false;
					}
				}

				int[] subUnits;
				char calendarUnit = schedule[firstIndex - 1];
				schedule = schedule.Substring(firstIndex + 1);
				// Month
				if (calendarUnit.ToString().ToUpper() == "M")
				{
					subUnits = GetSubUnits(schedule.Substring(0, schedule.IndexOf(':') - 2));

					foreach (int month in subUnits)
					{
						if (month == DateTime.Now.Month)
						{
							firstIndex = schedule.IndexOf(':');
							calendarUnit = schedule[firstIndex - 1];

							#region Week feature is complicated the scheduling so we removed it
							// Week
							//if (calendarUnit.ToString().ToUpper() == "W")
							//{
							//    schedule = schedule.Substring(firstIndex + 1);
							//    subUnits = GetSubUnits(schedule);

							//    foreach (int week in subUnits)
							//    {
							//        if (week == Math.Floor((double)(DateTime.Now.Day + 1) / 7) + 1)
							//        {
							//            firstIndex = schedule.IndexOf(':');
							//            calendarUnit = schedule[firstIndex - 1];

							//            if (calendarUnit.ToString().ToUpper() == "D")
							//            {
							//                subUnits = GetSubUnits(schedule);
							//                foreach (int day in subUnits)
							//                {
							//                    // DayOfWeek return values of 0-6 and because of it we -1 on left side.
							//                    if (day == (int)DateTime.Now.DayOfWeek)
							//                        return true;
							//                }
							//            }
							//        }
							//    }
							//}
							#endregion
							
							if (calendarUnit.ToString().ToUpper() == "D")
							{
								schedule = schedule.Substring(firstIndex + 1);
								subUnits = GetSubUnits(schedule);

								foreach (int day in subUnits)
								{
									if (day == DateTime.Now.Day)
										return true;
								}
							}
						}
					}
				}
				else if (calendarUnit.ToString().ToUpper() == "W")
				{
					subUnits = GetSubUnits(schedule);
					foreach (int day in subUnits)
					{
						// DayOfWeek return values of 0-6 and because of it we -1 on left side.
						if (day - 1 == (int)DateTime.Now.DayOfWeek)
							return true;
					}
				}
				else if (calendarUnit.ToString().ToUpper() == "D")
				{
					subUnits = GetSubUnits(schedule);
					foreach (int day in subUnits)
					{
						// DayOfWeek return values of 0-6 and because of it we -1 on left side.
						if (day == (int)DateTime.Now.Day)
							return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Return an int array with all the numbers that in the subUnits string.
		/// </summary>
		/// <param name="subUnits">a string the contain all the subUnits.</param>
		/// <returns>int array with all subUnits.</returns>
		private static int[] GetSubUnits(string subUnits)
		{
			int subUnit;
			int[] subUnitsIntArray = new int[50];
			int counter = 0;
			string[] subUnitsArray = subUnits.Split(',');
			foreach (string str in subUnitsArray)
			{
				if (Int32.TryParse(str, out subUnit))
					subUnitsIntArray[counter++] = subUnit;
				else
					Log.Write(string.Format("can't convert subunit {0} to int", str), LogMessageType.Warning);
			}
			return subUnitsIntArray;
		}
	}
}

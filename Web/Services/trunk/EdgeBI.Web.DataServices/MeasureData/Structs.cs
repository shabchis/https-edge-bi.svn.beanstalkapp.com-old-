using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using Easynet.Edge.Core.Utilities;

namespace EdgeBI.Web.DataServices
{
	[TypeConverter(typeof(DayCodeRange.Converter))]
	public struct DayCodeRange
	{
		public readonly int From;
		public readonly int To;
		public bool IsMainRange;

		public DayCodeRange(int from, int to):this()
		{
			From = from;
			To = to;
			IsMainRange = false;
		}

		#region Converter
		class Converter:TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				return sourceType == typeof(string);
			}
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			{
				return destinationType == typeof(string);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
			{
				string raw = value as string;
				
				bool isMain = false;
				if (raw.ToLower().Contains("(main)"))
				{
					isMain = true;
					raw = raw.Replace("(main)", string.Empty);
				}

				try
				{
					string[] dayCodes = raw.Split(new char[] { '-' }, 2);
					DayCodeRange range = new DayCodeRange(Int32.Parse(dayCodes[0]), Int32.Parse(dayCodes[1]));
					range.IsMainRange = isMain;
					return range;
				}
				catch (Exception ex)
				{
					throw new Exception("Invalid range: " + raw, ex);
				}
			}
		}
		#endregion
	}

	[TypeConverter(typeof(MeasureRef.Converter))]
	public struct MeasureRef
	{
		public int MeasureID;
		public bool IsTargetRef;
		public MeasureRef[] FunctionMeasures;

		public MeasureRef(int measureID, bool targetRef)
		{
			MeasureID = measureID;
			IsTargetRef = targetRef;
			FunctionMeasures = new MeasureRef[0];
		}
		
		#region Converter
		class Converter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				return sourceType == typeof(string);
			}
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			{
				return destinationType == typeof(string);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
			{
				string raw = value as string;
				try
				{
					string[] parts = raw.Split(new char[]{'('}, 2);//split using first peren
					string main = parts[0];
					bool isTarget = parts[0].ToLower().StartsWith("t"); //target mrefs start with t
					MeasureRef mref = new MeasureRef(
						Int32.Parse(isTarget ? main.Substring(1) : main),
						isTarget);
					if (parts.Length > 1)
					{
						string[] paramsRaw = parts[1].Substring(0,parts[1].Length-1).Split(',');//split over commas
						MeasureRef[] parameters = new MeasureRef[paramsRaw.Length];
						for (int i = 0; i < paramsRaw.Length; i++)
							parameters[i] = (MeasureRef) new Converter().ConvertFrom(paramsRaw[i].Trim());//convert each ref
						mref.FunctionMeasures = parameters;
					}

					return mref;
						
				}
				catch (Exception ex)
				{
					throw new Exception("Invalid measure ref: " + raw, ex);
				}
			}
		}
		#endregion
	}


	[TypeConverter(typeof(MeasureSort.Converter))]
	public struct MeasureSort
	{
		public int MeasureIndex;
		public int RangeIndex;
		public DiffType DiffType;
		public SortDir SortDir;
		
		#region Converter
		class Converter:TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				return sourceType == typeof(string);
			}
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			{
				return destinationType == typeof(string);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
			{
				string raw = value as string;
				try
				{
					string[] parts = raw.Split(new char[]{'('},2);
					MeasureSort msort = new MeasureSort(Int32.Parse(parts[0].Trim().Substring(1)), DiffType.None, SortDir.Desc);

					if (parts.Length > 1)
					{
						string[] parameters = parts[1].Substring(0, parts[1].Length - 1).Split(',');
						string[] diffNames = Enum.GetNames(typeof(DiffType));
						string[] dirNames = Enum.GetNames(typeof(SortDir));
						for (int i = 0; i < parameters.Length; i++)
						{
							if (diffNames.Contains(parameters[i], StringComparer.InvariantCultureIgnoreCase))
								msort.DiffType = (DiffType) Enum.Parse(typeof(DiffType), parameters[i], true);
							else if (dirNames.Contains(parameters[i], StringComparer.InvariantCultureIgnoreCase))
								msort.SortDir = (SortDir)Enum.Parse(typeof(SortDir), parameters[i], true);
						}
					}

					return msort;
				}
				catch (Exception ex)
				{
					throw new Exception("Invalid measure sort: " + raw, ex);
				}
			}
		}
		#endregion
	}

	public struct MeasureDiff
	{
		public int MeasureIndex;
		public DiffType DiffType;
	}

	public struct MeasureFormat
	{
		public int MeasureIndex;
		public bool FormatValue;
		public DiffType FormatDiffs;
	}

    public enum Mode
    {
        Simple,
        Advanced
    }

    public enum SortDir
	{
		Asc,
		Desc
	}

	public enum DataGrouping
	{
		Account,
		Channel,
		Campaign
	}

	public enum DiffType
	{
		None,
		DiffAbs,
		DiffRel,
		Both
	}


}

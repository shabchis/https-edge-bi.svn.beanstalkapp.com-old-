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

		public DayCodeRange(int from, int to):this()
		{
			From = from;
			To = to;
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

				try
				{
					string[] dayCodes = raw.Split(new char[] { '-' }, 2);
					DayCodeRange range = new DayCodeRange(Int32.Parse(dayCodes[0]), Int32.Parse(dayCodes[1]));
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
		public int DiffIndex;
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
					MeasureSort msort = new MeasureSort();

					// Segments
					string[] parts = raw.Split(new char[]{'.'},3);
					if (parts[0].ToLower()[0] == 'm')
						msort.MeasureIndex = Int32.Parse(parts[0].Substring(1));
					if (parts[1].ToLower()[0] == 'r')
						msort.RangeIndex = Int32.Parse(parts[1].Substring(1));
					if (parts.Length > 2)
					{
						string diffRaw = parts[2].Split(new char[] { '(' }, 2)[0];
						msort.DiffType = diffRaw.ToLower() == "value" ?
							DiffType.None :
							(DiffType)Enum.Parse(typeof(DiffType), diffRaw, true);
					}
                    //if(parts.
					else
						msort.DiffType = DiffType.None;

					// Direction
					parts = raw.Split(new char[] { '(' }, 2);
					msort.SortDir = (SortDir)Enum.Parse(typeof(SortDir), parts[1].Substring(0, parts[1].Length - 1), true);

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

	[TypeConverter(typeof(MeasureDiff.Converter))]
	public struct MeasureDiff
	{
		public int MeasureIndex;
		public DiffType DiffType;
		
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
					MeasureDiff mdiff = new MeasureDiff();

					// Segments
					string[] parts = raw.Split(new char[] { '(' }, 2);
					if (parts[0].ToLower()[0] == 'm')
						mdiff.MeasureIndex = Int32.Parse(parts[0].Substring(1));
					else if (parts[0].ToLower() == "all")
						mdiff.MeasureIndex = 0;
					else
						throw new FormatException("Measure index (e.g. 'm1' or 'all') is missing.");

					mdiff.DiffType = (DiffType)Enum.Parse(typeof(DiffType), parts[1].Substring(0, parts[1].Length - 1), true);

					return mdiff;
				}
				catch (Exception ex)
				{
					throw new Exception("Invalid measure diff: " + raw, ex);
				}
			}
		}
		#endregion
	}

	[TypeConverter(typeof(MeasureFormat.Converter))]
	public struct MeasureFormat
	{
		public int MeasureIndex;
		public bool ValueFormatting;
		public DiffType DiffFormatting;

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
					MeasureFormat mformat = new MeasureFormat();

					// Segments
					string[] parts = raw.Split(new char[] { '(' }, 2);
					if (parts[0].ToLower()[0] == 'm')
						mformat.MeasureIndex = Int32.Parse(parts[0].Substring(1));
					else if (parts[0].ToLower() == "all")
						mformat.MeasureIndex = -1;
					else
						throw new FormatException("Measure index (e.g. 'm1' or 'all') is missing.");

					string[] parameters = parts[1].Substring(0, parts[1].Length - 1).Split(new char[] { ',' }, 2);
					mformat.ValueFormatting = Boolean.Parse(parameters[0]);
					mformat.DiffFormatting = (DiffType)Enum.Parse(typeof(DiffType), parameters[1], true);

					return mformat;
				}
				catch (Exception ex)
				{
					throw new Exception("Invalid measure format: " + raw, ex);
				}
			}
		}
		#endregion
	}

    [TypeConverter(typeof(MeasureFilter.Converter))]
    public struct MeasureFilter
    {
        public int MeasureIndex;
        public int RangeIndex;
        public DiffType DiffType;
        public int FilterValue;
        public string Operator;

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
                    MeasureFilter mFilt = new MeasureFilter();
                    // Segments
                    string[] parts = raw.Split(new char[] { '.' }, 3);
                    if (parts[0].ToLower()[0] == 'm')
                        mFilt.MeasureIndex = Int32.Parse(parts[0].Substring(1));
                    if (parts[1].ToLower()[0] == 'r')
                        mFilt.RangeIndex = Int32.Parse(parts[1].Substring(1));
                    if (parts.Length > 2)
                    {
                        string diffRaw = parts[2].Split(new char[] { '<','>','=' }, 2)[0];
                        mFilt.DiffType = diffRaw.ToLower() == "value" ?
                            DiffType.None :
                            (DiffType)Enum.Parse(typeof(DiffType), diffRaw, true);
                    }
                    else
                        mFilt.DiffType = DiffType.None;
                    parts = raw.Split(new char[] { '<', '>', '=' }, 2);
                    mFilt.FilterValue = Convert.ToInt32(parts[1]);
                    if(raw.Contains(">")) mFilt.Operator = ">";
                    if(raw.Contains("<")) mFilt.Operator = "<";
                    if(raw.Contains("=")) mFilt.Operator = "=";

                    return mFilt;
                }
                catch (NullReferenceException )
                {
                    return null;
                }
                catch (Exception ex)
                {
                    throw new Exception("Invalid measure filter: " + raw, ex);
                }
            }
        }
        #endregion
    }

    public enum Mode
    {
        None,
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using Easynet.Edge.Core;
using System.ComponentModel;


namespace EdgeBI.Wizards
{
    /// <summary>
    /// New wizard parmeters, return when runing the start functionn
    /// </summary>
    [DataContract(Name = "WizardSession", Namespace = "EdgeBI.Wizards")]
    public struct WizardSession
    {
        /// <summary>
        /// The wizard number
        /// </summary>
        [DataMember]
        public int WizardID;
        /// <summary>
        /// The session id
        /// </summary>
        [DataMember]
        public int SessionID;
        [DataMember]
        public StepConfiguration CurrentStep;
    }
    /// <summary>
    /// Describe the current step and optionaly the next step fields
    /// </summary>
    /// 
    [TypeConverter(typeof(Replacment))]
    [KnownType(typeof(Replacment))]
    public class StepConfiguration
    {
        public string StepName;
        public Dictionary<string, object> MetaData;
    }

    /// <summary>
    /// The response returned when runing the collect function
    /// </summary>
    /// 
    
    [KnownType(typeof(Replacment))]
    public class StepCollectResponse
    {
        public StepResult Result;
        public StepConfiguration NextStep { get; set; } // Null if Result = HasErrors OR if there are no more steps
        public Dictionary<string, string> Errors;
    }
    /// <summary>
    /// Request the passed as paramter on the collect function
    /// </summary>
    /// 
    
    [KnownType(typeof(Replacment))]
    public class StepCollectRequest
    {
        public string StepName;
        public Dictionary<string, object> CollectedValues;
    }

    public struct ProgressState
    {
        public string text;
        public float OverAllProgess { get; set; }
        public Dictionary<string, float> CurrentRuningStepsState { get; set; }


    }
    [TypeConverter(typeof(Replacment.Converter))]
    public struct Replacment
    {
        public string ReplaceFrom { get; set; }
        public string ReplaceTo { get; set; }
        public bool CalcMembersOnly { get; set; }

        public override string ToString()
        {
            return string.Format("ReplaceFrom:{0};ReplaceTo:{1};CalcMembersOnly:{2}", ReplaceFrom, ReplaceTo, CalcMembersOnly);
        }

        public Replacment(string serialized)
        {
            try
            {
                this = new Replacment();
                SettingsCollection settings = new SettingsCollection(serialized);
                ReplaceFrom = settings["ReplaceFrom"];
                ReplaceTo = settings["ReplaceTo"];
                CalcMembersOnly = bool.Parse(settings["CalcMembersOnly"]);
            }
            catch (Exception ex)
            {

                throw new Exception("Can't convert from Value to Replacment object", ex);
            }
        }
        public class Converter : TypeConverter
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
                string serialized = value as string;
                Replacment replacmnet = new Replacment();
                return replacmnet = new Replacment(serialized);


            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                return value.ToString();
            }
        }
    }


    /// <summary>
    /// Result of specific request or respond
    /// </summary>
    public enum StepResult
    {
        Next,
        Done,
        HasErrors
    }
}
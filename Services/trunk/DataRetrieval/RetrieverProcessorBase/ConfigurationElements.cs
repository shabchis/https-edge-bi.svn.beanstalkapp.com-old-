using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Easynet.Edge.Services.DataRetrieval.Configuration
{
    // Fields
    public class FieldElement : ConfigurationElement
    {
        #region Member Variables
		/*=========================*/

        private static ConfigurationPropertyCollection s_properties;
        private static ConfigurationProperty s_key;
        private static ConfigurationProperty s_value;
        private static ConfigurationProperty s_enabled;
        private static ConfigurationProperty s_insertToDb;
		private static ConfigurationProperty s_DBFieldName;
		private static ConfigurationProperty s_requireChannel;
		private static ConfigurationProperty s_DBDefaultValue;
		private static ConfigurationProperty s_DBFieldType;
		private static ConfigurationProperty s_IsDimension;

		/*=========================*/
        #endregion

        #region Constructor
		/*=========================*/

        public FieldElement()
        {
            s_key = new ConfigurationProperty(
                "Key",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            s_value = new ConfigurationProperty(
                "Value",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            s_enabled = new ConfigurationProperty(
                "Enabled",
                typeof(bool),
                null,
				ConfigurationPropertyOptions.None);

			s_requireChannel = new ConfigurationProperty(
				"RequireChannel",
				typeof(bool),
				null,
				ConfigurationPropertyOptions.None);

			s_insertToDb = new ConfigurationProperty(
				"InsertToDB",
				typeof(bool),
				null,
				ConfigurationPropertyOptions.None);

			s_DBFieldName = new ConfigurationProperty(
				"DBFieldName",
				typeof(string),
				null,
				ConfigurationPropertyOptions.None);

			s_DBDefaultValue = new ConfigurationProperty(
				"DBDefaultValue",
				typeof(string),
				null,
				ConfigurationPropertyOptions.None);

			s_DBFieldType = new ConfigurationProperty(
				"DBFieldType",
				typeof(string),
				null,
				ConfigurationPropertyOptions.None);

			s_IsDimension = new ConfigurationProperty(
				"IsDimension",
				typeof(bool),
				false,
				ConfigurationPropertyOptions.None);

            s_properties = new ConfigurationPropertyCollection();
            s_properties.Add(s_key);
            s_properties.Add(s_value);
            s_properties.Add(s_enabled);
			s_properties.Add(s_insertToDb);
			s_properties.Add(s_DBFieldName);
			s_properties.Add(s_requireChannel);
			s_properties.Add(s_DBDefaultValue);
			s_properties.Add(s_DBFieldType);
			s_properties.Add(s_IsDimension);
        }

		/*=========================*/
        #endregion

        #region Properties
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return s_properties;
            }
        }

        public string Key
        {
            get
            {
                return (string)base[s_key];
            }
        }

        public string Value
        {
            get
            {
                return (string)base[s_value];
            }
        }

        public bool Enabled
        {
            get
            {
                return (bool)base[s_enabled];
            }
        }

		public bool RequireChannel
		{
			get
			{
				return (bool)base[s_requireChannel];
			}
		}

		public bool InsertToDB
		{
			get
			{
				return (bool)base[s_insertToDb];
			}
		}

		public string DBFieldName
		{
			get
			{
				return (string)base[s_DBFieldName];
			}
		}

		public string DBDefaultValue
		{
			get
			{
				return (string)base[s_DBDefaultValue];
			}
		}

		public string DBFieldType
		{
			get
			{
				return (string)base[s_DBFieldType];
			}
		}

		public bool IsDimension
		{
			get
			{
				return (bool)base[s_IsDimension];
			}
		}

        #endregion

		// Yaniv: probebly can remove.
        public object GetValueAsType()
        {
			//if (!InsertToDB)
			//{
			//    return null;
			//}

			//if (DBType == null)
			//{
			//    return null;
			//}

            //TODO:
            //Return the value based on the database type.
            //First validate.
            //Second, convert to the relevant DB Type.

            return null;
        }
    }

	// Fields Collection
    public class FieldElementCollection : ConfigurationElementCollection
    {
        #region Constructor
        public FieldElementCollection()
        {
        }
        #endregion

        #region Properties
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "Field";
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return new ConfigurationPropertyCollection();
            }
        }
        #endregion

        #region Indexers
        public FieldElement this[int index]
        {
            get
            {
                return (FieldElement)base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new FieldElement this[string name]
        {
            get
            {
                return (FieldElement)base.BaseGet(name);
            }
        }
        #endregion

        #region Methods
        public void Add(FieldElement item)
        {
            base.BaseAdd(item);
        }

        public void Remove(FieldElement item)
        {
            base.BaseRemove(item);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }
        #endregion

        #region Overrides
        protected override ConfigurationElement CreateNewElement()
        {
			return new FieldElement();		
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as FieldElement).Key;
        }
        #endregion
    }

	// Field section
    public class FieldElementSection : ConfigurationSection
    {
        #region Member Variables
        private static ConfigurationPropertyCollection s_properties;
        private static ConfigurationProperty s_fields;
        #endregion

        #region Constructor
        static FieldElementSection()
        {
            s_fields = new ConfigurationProperty(
                "",
                typeof(FieldElementCollection),
                null,
                ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection);

            s_properties = new ConfigurationPropertyCollection();
            s_properties.Add(s_fields);
        }
        #endregion

        #region Properties
        public FieldElementCollection Fields
        {
            get { return (FieldElementCollection)base[s_fields]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return s_properties;
            }
        }
        #endregion
    }

    // ReportTypes
    public class ReportTypesElement : ConfigurationElement
    {
        #region Member Variables
        private static ConfigurationPropertyCollection s_properties;
        private static ConfigurationProperty s_type;
        private static ConfigurationProperty s_aggregation;
        private static ConfigurationProperty s_name;
        private static ConfigurationProperty s_enabled;
        #endregion

        #region Constructor
        public ReportTypesElement()
        {
			lock (this)
			{
				s_type = new ConfigurationProperty(
					"Type",
					typeof(string),
					null,
					ConfigurationPropertyOptions.IsRequired);

				s_aggregation = new ConfigurationProperty(
					"Aggregation",
					typeof(string),
					null,
					ConfigurationPropertyOptions.IsRequired);

				s_name = new ConfigurationProperty(
					"Name",
					typeof(string),
					null,
					ConfigurationPropertyOptions.IsRequired);

				s_enabled = new ConfigurationProperty(
				 "Enabled",
				 typeof(bool),
				 null,
				 ConfigurationPropertyOptions.IsRequired);

				s_properties = new ConfigurationPropertyCollection();
				s_properties.Add(s_type);
				s_properties.Add(s_aggregation);
				s_properties.Add(s_name);
				s_properties.Add(s_enabled);
			}
        }
        #endregion

        #region Properties
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return s_properties;
            }
        }

        public string Type
        {
            get
            {
                return (string)base[s_type];
            }
        }

        public string Aggregation
        {
            get
            {
                return (string)base[s_aggregation];
            }
        }

        public string Name
        {
            get
            {
                return (string)base[s_name];
            }
        }

        public bool Enabled
        {
            get
            {
                return (bool)base[s_enabled];
            }
        }
        #endregion
    }

	// ReportTypes Collection
    public class ReportTypesElementCollection : ConfigurationElementCollection
    {
        #region Constructor
        public ReportTypesElementCollection()
        {
        }
        #endregion

        #region Properties
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "ReportType";
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return new ConfigurationPropertyCollection();
            }
        }
        #endregion

        #region Indexers
        public ReportTypesElement this[int index]
        {
            get
            {
                return (ReportTypesElement)base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new ReportTypesElement this[string name]
        {
            get
            {
                return (ReportTypesElement)base.BaseGet(name);
            }
        }
        #endregion

        #region Methods
        public void Add(ReportTypesElement item)
        {
            base.BaseAdd(item);
        }

        public void Remove(ReportTypesElement item)
        {
            base.BaseRemove(item);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }
        #endregion

        #region Overrides
        protected override ConfigurationElement CreateNewElement()
        {
            return new ReportTypesElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ReportTypesElement).Name;
        }
        #endregion
    }

	// ReportTypes Section
    public class ReportTypesElementSection : ConfigurationSection
    {
        #region Member Variables
        private static ConfigurationPropertyCollection s_properties;
        private static ConfigurationProperty s_fields;
        #endregion

        #region Constructor
        static ReportTypesElementSection()
        {
            s_fields = new ConfigurationProperty(
                "",
                typeof(ReportTypesElementCollection),
                null,
                ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection);

            s_properties = new ConfigurationPropertyCollection();
            s_properties.Add(s_fields);
        }
        #endregion

        #region Properties
        public ReportTypesElementCollection Fields
        {
            get { return (ReportTypesElementCollection)base[s_fields]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return s_properties;
            }
        }
        #endregion
    }
}

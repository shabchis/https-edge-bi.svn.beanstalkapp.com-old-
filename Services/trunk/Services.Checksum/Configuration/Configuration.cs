using System;
using System.Configuration;
using Easynet.Edge.Core.Configuration;
using System.Xml;

namespace Easynet.Edge.Services.Checksum.Configuration
{
	/// <summary>
	/// Represents a collection of directories to monitor.
	/// </summary>
	public class PhaseElementCollection: ConfigurationElementCollectionBase<PhaseElement>
	{
		#region Constructor
		/*=========================*/
		public PhaseElementCollection()
		{
		}
		/*=========================*/
		#endregion

		#region Properties
		/*=========================*/
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
				return "Phase";
			}
		}
		/*=========================*/
		#endregion

		#region Indexers
		/*=========================*/
		public new PhaseElement this[string name]
		{
			get
			{
				return (PhaseElement) base.BaseGet(name);
			}
		}
		/*=========================*/
		#endregion

		#region Methods
		/*=========================*/
		public void Add(PhaseElement item)
		{
			base.BaseAdd(item);
		}

		public void Remove(PhaseElement item)
		{
			base.BaseRemove(item);
		}

		public void RemoveAt(int index)
		{
			base.BaseRemoveAt(index);
		}
		/*=========================*/
		#endregion

		#region Overrides
		/*=========================*/
		protected override ConfigurationElement CreateNewElement()
		{
			return new PhaseElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			// Get index name
			return (element as PhaseElement).PhaseName;
		}
		/*=========================*/
		#endregion
	}

	/// <summary>
	/// Represents a directory defintion for the file system watcher
	/// </summary>
	public class PhaseElement: ReferencingConfigurationElement
	{
		#region Members
		/*=========================*/
		private ConfigurationProperty s_name;
		private ConfigurationProperty s_handlerType;
        /*=========================*/
		#endregion

		#region Constructor
		/*=========================*/
		public PhaseElement()
		{
			s_name = new ConfigurationProperty(
				"Name",
				typeof(string),
				null,
				ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

			s_handlerType = new ConfigurationProperty(
				"HandlerType",
				typeof(string),
				null,
				ConfigurationPropertyOptions.IsRequired);

			InnerProperties.Add(s_name);
			InnerProperties.Add(s_handlerType);
		}
		/*=========================*/
		#endregion

		#region Properties
		/*=========================*/
		public string Name
		{
			get
			{
				return (string)base[s_name];
			}
			set
			{
				base[s_name] = value;
			}
		}

		public string HandlerType
		{
			get
			{
				return (string)base[s_handlerType];
			}
			set
			{
				base[s_handlerType] = value;
			}
		}

        /*=========================*/
		#endregion
	}
}

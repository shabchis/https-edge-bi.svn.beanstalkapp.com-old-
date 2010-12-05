using System;
using System.Configuration;
using Easynet.Edge.Core.Configuration;
using System.Xml;

namespace Easynet.Edge.Services.FileImport.Configuration
{
	/// <summary>
	/// Represents a collection of directories to monitor.
	/// </summary>
	public class DirectoryElementCollection: ConfigurationElementCollectionBase<DirectoryElement>
	{
		#region Constructor
		/*=========================*/
		public DirectoryElementCollection()
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
				return "Directory";
			}
		}
		/*=========================*/
		#endregion

		#region Indexers
		/*=========================*/
		public new DirectoryElement this[string name]
		{
			get
			{
				return (DirectoryElement) base.BaseGet(name);
			}
		}
		/*=========================*/
		#endregion

		#region Methods
		/*=========================*/
		public void Add(DirectoryElement item)
		{
			base.BaseAdd(item);
		}

		public void Remove(DirectoryElement item)
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
			return new DirectoryElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			// Get index name
			return (element as DirectoryElement).Path;
		}
		/*=========================*/
		#endregion
	}

	/// <summary>
	/// Represents a directory defintion for the file system watcher
	/// </summary>
	public class DirectoryElement: ReferencingConfigurationElement
	{
		#region Members
		/*=========================*/
		private ConfigurationProperty s_path;
		private ConfigurationProperty s_filter;
		private ConfigurationProperty s_handler;
		private ConfigurationProperty s_includeSubdirs;
        private ConfigurationProperty s_handlerParameters;
        /*=========================*/
		#endregion

		#region Constructor
		/*=========================*/
		public DirectoryElement()
		{
			s_path = new ConfigurationProperty(
				"Path",
				typeof(string),
				null,
				ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

			/*
			s_handler = new ConfigurationProperty(
				"HandlerService",
				typeof(ElementReference<ServiceElement>),
				new ElementReference<ServiceElement>(),
				new Easynet.Edge.Core.Configuration.Converters.ElementReferenceConverter<ServiceElement>(this, "HandlerService"),
				null,
				ConfigurationPropertyOptions.IsRequired);
			*/

			s_handler = new ConfigurationProperty(
				"HandlerService",
				typeof(string),
				null,
				ConfigurationPropertyOptions.IsRequired);


			s_filter = new ConfigurationProperty(
				"Filter",
				typeof(string));

			s_includeSubdirs = new ConfigurationProperty(
				"IncludeSubdirectories",
				typeof(bool),
				true);

            s_handlerParameters = new ConfigurationProperty(
                "HandlerServiceParameters",
                typeof(string),
                String.Empty);

			InnerProperties.Add(s_path);
			InnerProperties.Add(s_handler);
			InnerProperties.Add(s_filter);
			InnerProperties.Add(s_includeSubdirs);
            InnerProperties.Add(s_handlerParameters);
		}
		/*=========================*/
		#endregion

		#region Properties
		/*=========================*/
		public string Path
		{
			get
			{
				return (string) base[s_path];
			}
			set
			{
				base[s_path] = value;
			}
		}

		public string Filter
		{
			get
			{
				return (string) base[s_filter];
			}
			set
			{
				base[s_filter] = value;
			}
		}

		public bool IncludeSubdirs
		{
			get
			{
				return (bool) base[s_includeSubdirs];
			}
			set
			{
				base[s_includeSubdirs] = value;
			}
		}

		public string HandlerService
		{
			get
			{
				return (string)base[s_handler];
			}
			set
			{
				base[s_handler] = value;
			}
		}

        public string HandlerServiceParameters
        {
            get
            {
                return (string)base[s_handlerParameters];
            }
            set
            {
                base[s_handlerParameters] = value;
            }
        }
        /*=========================*/
		#endregion
	}
}

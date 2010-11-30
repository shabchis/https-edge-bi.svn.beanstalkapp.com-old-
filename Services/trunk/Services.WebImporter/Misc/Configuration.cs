using System;
using System.Configuration;
using Easynet.Edge.Core.Configuration;
using System.Xml;

namespace Easynet.Edge.Services.WebImporter.Configuration
{
	/// <summary>
	/// Represents a collection of directories to monitor.
	/// </summary>
	public class FileTypeElementCollection : ConfigurationElementCollectionBase<FileTypeElement>
	{
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
				return "FileType";
			}
		}

		public new FileTypeElement this[string name]
		{
			get
			{
				return (FileTypeElement) base.BaseGet(name);
			}
		}

		public void Add(FileTypeElement item)
		{
			base.BaseAdd(item);
		}

		public void Remove(FileTypeElement item)
		{
			base.BaseRemove(item);
		}

		public void RemoveAt(int index)
		{
			base.BaseRemoveAt(index);
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new FileTypeElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			// Get index name
			return (element as FileTypeElement).Extension;
		}

	}

	/// <summary>
	/// Represents a directory defintion for the file system watcher
	/// </summary>
	public class FileTypeElement: ReferencingConfigurationElement
	{

		public FileTypeElement()
		{
			s_extension = new ConfigurationProperty(
				"Extension",
				typeof(string),
				null,
				ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

			s_connectionString = new ConfigurationProperty(
				"ConnectionString",
				typeof(string));

			InnerProperties.Add(s_extension);
			InnerProperties.Add(s_connectionString);
		}

		private ConfigurationProperty s_extension;
		public string Extension
		{
			get
			{
				return (string)base[s_extension];
			}
			set
			{
				base[s_extension] = value;
			}
		}

		private ConfigurationProperty s_connectionString;
		public string ConnectionString
		{
			get
			{
				return (string)base[s_connectionString];
			}
			set
			{
				base[s_connectionString] = value;
			}
		}

	}
}

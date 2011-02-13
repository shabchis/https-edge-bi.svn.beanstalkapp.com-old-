using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Data;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using System.Collections;

namespace EdgeBI.Objects
{
	/// <summary>
	/// Return objects dynamicly
	/// </summary>
	class MapperUtility
	{
		#region Public methods
		/// <summary>
		/// Return SingleObject
		/// </summary>
		/// <typeparam name="ThingT"></typeparam>
		/// <param name="Key"></param>
		/// <param name="onApplyValue"></param>
		/// <returns></returns>
		public static ThingT CreateMainObject<ThingT>(IDataReader sqlDataReader, Func<FieldInfo, IDataRecord, object> onApplyValue) where ThingT : class, new()
		{
			Type t = typeof(ThingT);
			ThingT returnObject = (ThingT)Activator.CreateInstance(t);


			foreach (FieldInfo fieldInfo in t.GetFields())
			{
				object val;
				if (Attribute.IsDefined(fieldInfo, typeof(FieldMapAttribute)))
				{
					FieldMapAttribute fieldMapAttribute = (FieldMapAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(FieldMapAttribute));

					if (fieldMapAttribute.Show)
					{


						if (sqlDataReader.FieldExists(fieldMapAttribute.FieldName))
						{
							if (fieldMapAttribute.UseApplyFunction && onApplyValue != null)
								val = onApplyValue(fieldInfo, sqlDataReader);
							else
								val = sqlDataReader[fieldMapAttribute.FieldName];
							if (val is DBNull)
							{
								fieldInfo.SetValue(returnObject, null);
							}
							else
								fieldInfo.SetValue(returnObject, val);
						}

					}
				}
			}
			return returnObject;

		}
		public static int SaveOrRemoveSimpleObject<T>(string Command, CommandType commandType, SqlOperation sqlOperation, object objectToInsert, string connectionString) where T : class, new()
		{
			int rowsAfected = 0;
			Type t = typeof(T);
			

			if (!string.IsNullOrEmpty(connectionString))
				connectionString = DataManager.ConnectionString;


			using (SqlConnection sqlConnection=new SqlConnection(connectionString)  )
			{
				using (SqlCommand sqlCommand = DataManager.CreateCommand(Command, commandType))
				{
					if (sqlCommand.Parameters.Contains("@Action"))
						sqlCommand.Parameters["@Action"].Value = sqlOperation;
					foreach (FieldInfo fieldInfo in objectToInsert.GetType().GetFields())
					{
						if (Attribute.IsDefined(fieldInfo, typeof(FieldMapAttribute)))
						{
							FieldMapAttribute fieldMapAttribute = (FieldMapAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(FieldMapAttribute));
							if (sqlCommand.Parameters.Contains(string.Format("@{0}", fieldMapAttribute.FieldName)))
							{
								if (fieldInfo.GetValue(objectToInsert) != null)
									sqlCommand.Parameters[string.Format("@{0}", fieldMapAttribute.FieldName)].Value = fieldInfo.GetValue(objectToInsert);
								else
									sqlCommand.Parameters[string.Format("@{0}", fieldMapAttribute.FieldName)].Value = DBNull.Value;
							}
						}
					}
					rowsAfected = sqlCommand.ExecuteNonQuery();
				}
			}
			return rowsAfected;

		}
		public static T ExpandObject<T>(object o, Func<FieldInfo, IDataRecord, object> customApply) where T : class , new()
		{
			Type t = typeof(T);
			T returnObject = (T)o;
			string dictionaryKey = string.Empty;
			SqlCommand sqlCommand;

			foreach (FieldInfo fieldInfo in returnObject.GetType().GetFields())
			{
				if (Attribute.IsDefined(fieldInfo, typeof(DictionaryMapAttribute)))
				{
					DictionaryMapAttribute dictionaryMapAttribute = (DictionaryMapAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DictionaryMapAttribute));
					using (DataManager.Current.OpenConnection())
					{
						if (!dictionaryMapAttribute.IsStoredProcedure)
							sqlCommand = DataManager.CreateCommand(dictionaryMapAttribute.Command);
						else
							sqlCommand = DataManager.CreateCommand(dictionaryMapAttribute.Command, CommandType.StoredProcedure);
						foreach (SqlParameter param in sqlCommand.Parameters)
						{
							string fieldName = param.ParameterName.Substring(1); //without the "@"
							param.Value = returnObject.GetType().GetField(fieldName).GetValue(returnObject);

						}

						fieldInfo.SetValue(returnObject, GetDictionryObject(fieldInfo, sqlCommand.ExecuteReader()));
					}
				}
			}

			return (T)returnObject;
		}
		public static IList GetMenus<T>(object Key, bool recursive, Func<FieldInfo, IDataRecord, object> onApplyValue) where T : class, new()
		{
			StringBuilder cmdMainObject = new StringBuilder();
			string tableName = string.Empty;
			string KeyFieldName = string.Empty;
			string KeyFieldType = string.Empty;
			Type requestedType = typeof(T);
			IList returnObject;
			Type typeElement;

			//init
			if (requestedType.IsGenericType)
			{
				returnObject = (IList)Activator.CreateInstance(requestedType);
				typeElement = returnObject.GetType().GetGenericArguments()[0];
				if (Attribute.IsDefined(typeElement, typeof(TableMapAttribute)))
				{

					TableMapAttribute tableMapAttribute = (TableMapAttribute)Attribute.GetCustomAttribute(typeElement, typeof(TableMapAttribute));
					tableName = tableMapAttribute.TableName; //Map the table name for the query

				}
				else
					throw new InvalidOperationException("Class must have TableMap attribute defined");

			}
			else
				throw new Exception("This is not generic type");
			//Create query
			//Table


			List<FieldInfo> regularMapedFields = new List<FieldInfo>();
			foreach (FieldInfo fieldInfo in typeElement.GetFields())
			{

				if (Attribute.IsDefined(fieldInfo, typeof(FieldMapAttribute))) //Check if the field is a table field if it is use it on the query
				{
					FieldMapAttribute fieldMapAttribute = (FieldMapAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(FieldMapAttribute));
					regularMapedFields.Add(fieldInfo);
					if (fieldMapAttribute.IsKey)
					{
						KeyFieldName = fieldMapAttribute.FieldName;
						KeyFieldType = Easynet.Edge.Core.Utilities.TypeConvertor.ToSqlDbType(fieldInfo.FieldType).ToString();
					}

				}

			}
			cmdMainObject.AppendLine("SELECT ");
			cmdMainObject.Append(CreateSelectList(regularMapedFields));

			cmdMainObject.AppendLine(string.Format(" FROM {0} ", tableName));
			//Add the where clause
			if (Key == null)
			{
				cmdMainObject.AppendLine(string.Format(" WHERE {0}  is null", KeyFieldName));
			}
			else
			{
				cmdMainObject.AppendLine(string.Format(" WHERE {0}=@PrimeryKey:{1} ", KeyFieldName, KeyFieldType));
			}

			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand = DataManager.CreateCommand(cmdMainObject.ToString()))
				{
					if (Key != null)
						sqlCommand.Parameters["@PrimeryKey"].Value = Key;


					using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
					{
						while (sqlDataReader.Read())
						{
							object currentItem = Activator.CreateInstance(typeElement);
							foreach (FieldInfo fieldInfo in typeElement.GetFields())
							{

								if (Attribute.IsDefined(fieldInfo, typeof(FieldMapAttribute)))
								{
									FieldMapAttribute fieldMapAttribute = (FieldMapAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(FieldMapAttribute));
									object val;
									if (fieldMapAttribute.UseApplyFunction && onApplyValue != null)
										val = onApplyValue(fieldInfo, sqlDataReader);
									else
										val = sqlDataReader[fieldMapAttribute.FieldName];
									if (val is DBNull)
									{
										fieldInfo.SetValue(currentItem, null);
									}
									else
									{
										fieldInfo.SetValue(currentItem, val);
									}
								}
							}
							returnObject.Add(currentItem);
						}
					}
				}
			}
			if (recursive)
			{

				object recursiveLookupValue = null;
				for (int i = 0; i < returnObject.Count; i++)
				{
					object currentItem = Activator.CreateInstance(returnObject[i].GetType());
					Type currentType = currentItem.GetType();
					FieldInfo childsField = null;
					currentItem = returnObject[i];
					foreach (FieldInfo fieldInfo in currentType.GetFields())
					{
						if (Attribute.IsDefined(fieldInfo, typeof(FieldMapAttribute)))
						{
							FieldMapAttribute fieldMapAttribute = (FieldMapAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(FieldMapAttribute));
							if (fieldMapAttribute.RecursiveLookupField)
								recursiveLookupValue = fieldInfo.GetValue(currentItem);

						}
						if (Attribute.IsDefined(fieldInfo, typeof(HasChildOfTheSameAttribute)))
							childsField = fieldInfo;

					}
					if (recursiveLookupValue != null)
					{
						if (childsField != null)
							childsField.SetValue(currentItem, GetMenus<T>(recursiveLookupValue, recursive, onApplyValue));
					}
				}
			}
			return returnObject;

		}
		//public static object GetListObjectByKey(FieldInfo fieldInfo, object primeryKey)
		//{
		//    StringBuilder cmdListObject = new StringBuilder();
		//    string tableName = string.Empty;
		//    List<FieldInfo> regularMapedFields = new List<FieldInfo>();
		//    string whereClause = string.Empty;
		//    string orderbyClause = string.Empty;
		//    string fromClouse = string.Empty;




		//    if (!fieldInfo.FieldType.IsGenericType || fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(IList))
		//        throw new Exception("This is not generic list");


		//    if (Attribute.IsDefined(fieldInfo, typeof(ListMapAttribute)))
		//    {
		//        ListMapAttribute listMapAttribute = (ListMapAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(ListMapAttribute));
		//        if (!string.IsNullOrEmpty(listMapAttribute.WhereClause))
		//            whereClause = string.Format("WHERE {0} ", listMapAttribute.WhereClause);
		//        if (!string.IsNullOrEmpty(listMapAttribute.OrderBy))
		//            orderbyClause = string.Format("ORDER BY {0} ", listMapAttribute.OrderBy);
		//        tableName = listMapAttribute.TableName;


		//    }
		//    else
		//        throw new Exception("listmapatrribute not defined");


		//    Type typeElement = fieldInfo.FieldType.GetGenericArguments()[0];


		//    foreach (FieldInfo f in typeElement.GetFields())  //
		//    {

		//        if (Attribute.IsDefined(f, typeof(FieldMapAttribute))) //Check if the field is a table field if it is use it on the query
		//        {
		//            regularMapedFields.Add(f);
		//        }
		//    }
		//    cmdListObject.AppendLine("SELECT ");
		//    cmdListObject.AppendLine(CreateSelectList(regularMapedFields));




		//    //Add the from Clause
		//    cmdListObject.AppendLine(string.Format(" FROM {0} ", tableName));

		//    //Add the where clause
		//    if (!string.IsNullOrEmpty(whereClause))
		//        cmdListObject.AppendLine(whereClause);
		//    if (!string.IsNullOrEmpty(orderbyClause))
		//        cmdListObject.Append(orderbyClause);


		//    IList returnObject = (IList)Activator.CreateInstance(fieldInfo.FieldType);
		//    using (DataManager.Current.OpenConnection())
		//    {
		//        using (SqlCommand sqlCommand = DataManager.CreateCommand(cmdListObject.ToString()))
		//        {
		//            //sqlcommand parameters
		//            sqlCommand.Parameters["@PrimeryKey"].Value = primeryKey;
		//            using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
		//            {
		//                while (sqlDataReader.Read())
		//                {
		//                    object currentItem = Activator.CreateInstance(typeElement);

		//                    foreach (FieldInfo f in typeElement.GetFields())
		//                    {
		//                        if (Attribute.IsDefined(f, typeof(FieldMapAttribute)))
		//                        {
		//                            FieldMapAttribute fieldMapAttribute = (FieldMapAttribute)Attribute.GetCustomAttribute(f, typeof(FieldMapAttribute));
		//                            object val = sqlDataReader[fieldMapAttribute.FieldName];
		//                            if (val is DBNull)
		//                            {
		//                                f.SetValue(currentItem, null);
		//                            }
		//                            else
		//                                f.SetValue(currentItem, val);

		//                        }

		//                    }
		//                    returnObject.Add(currentItem);
		//                }
		//            }
		//        }
		//    }

		//    return returnObject;
		//}
		public static IList GetListObject(FieldInfo fieldInfo, IDataReader sqlDataReader)
		{
			if (!fieldInfo.FieldType.IsGenericType || fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(IList))
				throw new Exception("This is not generic list");
			Type typeElement = fieldInfo.FieldType.GetGenericArguments()[0];
			IList returnObject = (IList)Activator.CreateInstance(fieldInfo.GetType());
			//Get the inner type

			while (sqlDataReader.Read())
			{
				object currentItem = Activator.CreateInstance(typeElement);
				foreach (FieldInfo f in typeElement.GetFields())
				{
					if (Attribute.IsDefined(f, typeof(FieldMapAttribute)))
					{
						FieldMapAttribute fieldMapAttribute = (FieldMapAttribute)Attribute.GetCustomAttribute(f, typeof(FieldMapAttribute));
						object val = sqlDataReader[fieldMapAttribute.FieldName];
						if (val is DBNull)
						{
							f.SetValue(currentItem, null);
						}
						else
							f.SetValue(currentItem, val);
					}
				}
				returnObject.Add(currentItem);
			}
			return returnObject;
		}
		public static IDictionary GetDictionryObject(FieldInfo fieldInfo, IDataReader sqlDataReader)
		{
			string dictionaryKey = string.Empty;
			if (!fieldInfo.FieldType.IsGenericType || fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(IDictionary))
				throw new Exception("This is not generic Dictionary");

			Type keyElement = fieldInfo.FieldType.GetGenericArguments()[0];
			Type typeElement = fieldInfo.FieldType.GetGenericArguments()[1];

			if (Attribute.IsDefined(fieldInfo, typeof(DictionaryMapAttribute)))
			{
				DictionaryMapAttribute dictionaryMapAttribute = (DictionaryMapAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DictionaryMapAttribute));
				dictionaryKey = dictionaryMapAttribute.DictionaryKey;
			}
			else
				throw new Exception("DictionaryMapatrribute not defined");


			IDictionary returnObject = (IDictionary)Activator.CreateInstance(fieldInfo.FieldType);
			if (typeElement.IsGenericType)
			{
				IList list = (IList)Activator.CreateInstance(typeElement);
				int? lastAccountId = null;
				while (sqlDataReader.Read())
				{
					object currentItem = Activator.CreateInstance(typeElement.GetGenericArguments()[0]);
					if (lastAccountId != null)
					{
						if (lastAccountId != (int)sqlDataReader[dictionaryKey])
						{
							returnObject.Add(lastAccountId, list);
							list = (IList)Activator.CreateInstance(typeElement);
						}

					}
					foreach (FieldInfo f in typeElement.GetGenericArguments()[0].GetFields())
					{
						if (Attribute.IsDefined(f, typeof(FieldMapAttribute)))
						{
							FieldMapAttribute fieldMapAttribute = (FieldMapAttribute)Attribute.GetCustomAttribute(f, typeof(FieldMapAttribute));
							object val = sqlDataReader[fieldMapAttribute.FieldName];
							if (val is DBNull)
							{
								f.SetValue(currentItem, null);
							}
							else
								f.SetValue(currentItem, val);
						}
					}
					list.Add(currentItem);
					lastAccountId = (int)sqlDataReader[dictionaryKey];
				}
				if (lastAccountId != null)
				{
					returnObject.Add(lastAccountId, list);
				}
			}
			return returnObject;
		}

		#endregion
		#region Private Methods
		private static string CreateSelectList(List<FieldInfo> RegularMapedFields)
		{
			StringBuilder selectClause = new StringBuilder();
			FieldMapAttribute fieldMapAttribute;
			foreach (FieldInfo fieldInfo in RegularMapedFields)
			{
				fieldMapAttribute = (FieldMapAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(FieldMapAttribute));
				if (fieldMapAttribute.FieldName != null)
				{
					if (fieldMapAttribute.IsDistinct)
						selectClause.Append("DISTINCT ");
					if (string.IsNullOrEmpty(fieldMapAttribute.Cast))
					{
						selectClause.Append(string.Format("{0},", fieldMapAttribute.FieldName));
					}
					else
					{
						selectClause.Append(string.Format("{0},", fieldMapAttribute.Cast));
					}
				}
			}
			//Clean last ','
			selectClause = selectClause.Remove(selectClause.Length - 1, 1);
			return selectClause.ToString();
		}
		#endregion


	}
	#region AtrributesClass
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	sealed class FieldMapAttribute : Attribute
	{
		// See the attribute guidelines at 
		//  http://go.microsoft.com/fwlink/?LinkId=85236
		public readonly string FieldName;
		public string Cast { get; set; }
		public bool IsKey { get; set; }
		public bool Show = true;
		public bool UseApplyFunction { get; set; }
		public bool RecursiveLookupField { get; set; }
		public bool IsDistinct { get; set; }

		// This is a positional argument
		public FieldMapAttribute(string fieldName)
		{
			this.FieldName = fieldName;

		}
	}

	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	sealed class TableMapAttribute : Attribute
	{
		// See the attribute guidelines at 
		//  http://go.microsoft.com/fwlink/?LinkId=85236
		public readonly string TableName;

		// This is a positional argument
		public TableMapAttribute(string tableName)
		{
			this.TableName = tableName;
		}
	}
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	sealed class DictionaryMapAttribute : Attribute
	{
		public string Command { get; set; }
		public string DictionaryKey { get; set; }
		public bool IsStoredProcedure { get; set; }

	}
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	sealed class ListMapAttribute : Attribute
	{
		public string Command { get; set; }
		public string DictionaryKey { get; set; }
		public bool IsStoredProcedure { get; set; }
	}

	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	sealed class HasChildOfTheSameAttribute : Attribute
	{

	}

	#endregion
	public static class DataReaderExtensions
	{
		public static bool FieldExists(this IDataReader reader, string fieldName)
		{
			reader.GetSchemaTable().DefaultView.RowFilter = string.Format("ColumnName= '{0}'", fieldName);
			return (reader.GetSchemaTable().DefaultView.Count > 0);
		}
	}
}

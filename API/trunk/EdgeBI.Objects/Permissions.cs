using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;


namespace EdgeBI.Objects
{
	//[TableMap("User_GUI_User")]
	public class AssignedPermission
	{
		[DataMember]
		[FieldMap("PermissionType")]
		public string Path;

		[DataMember]
		[FieldMap("Value",Cast="cast (Value  as int) AS 'Value' ")]
		public PermissionAssignmentType Assignment;

	}
	public class CalculatedPermission
	{
		[DataMember]
		[FieldMap("AccountID")]
		public int AccountID;
		[DataMember]
		[FieldMap("PermissionType")]
		public string Path;

	}
	public class PermissionRequest
	{
		public int AccountID { get; set; }
		public string Path { get; set; }	
	}

	public enum PermissionAssignmentType
	{
		Allow = 1,
		Deny = 0
	}

}

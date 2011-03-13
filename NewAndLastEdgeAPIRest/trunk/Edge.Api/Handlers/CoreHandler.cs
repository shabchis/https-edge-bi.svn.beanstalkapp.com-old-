using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Edge.Api.Handlers.UriTemplate;
using Edge.Objects;
using System.Data;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Utilities;
using System.Data.SqlClient;

namespace Edge.Api.Handlers
{
	public class CoreHandler: UriTemplateHandler
	{
		private const string KeyEncrypt = "5c51374e366f41297356413c71677220386c534c394742234947567840";
		public override bool ShouldValidateSession
		{
			get { return false; }
		}

		[UriMapping(Method = "POST", Template = "users/{userID}/permissions", BodyParameter="jsonObject")]
		public object Example(int userID, object jsonObject)
		{
			return null;
		}


		#region Login
		[UriMapping(Method = "POST", Template = "sessions", BodyParameter = "sessionData")]
		public SessionResponseData LogIn(SessionRequestData sessionData)
		{
			SqlCommand sqlCommand;
			SessionResponseData returnsessionData = null;
			int session;
			throw new Exception("vdvsd");
			try
			{
				using (DataManager.Current.OpenConnection())
				{
					Encryptor encryptor = new Encryptor(KeyEncrypt);
					sqlCommand = DataManager.CreateCommand("User_Login(@OperationType:Int,@Email:NVarchar,@Password:NVarchar,@UserID:Int,@SessionID:Int)", CommandType.StoredProcedure);


					sqlCommand.Parameters["@OperationType"].Value = sessionData.OperationType;
					if (sessionData.OperationType == OperationTypeEnum.New)
					{
						sqlCommand.Parameters["@Email"].Value = sessionData.Email;
						sqlCommand.Parameters["@Password"].Value = sessionData.Password;
					}
					else
					{
						sqlCommand.Parameters["@UserID"].Value = sessionData.UserID;

						try
						{
							sqlCommand.Parameters["@SessionID"].Value = encryptor.Decrypt(sessionData.Session);
						}
						catch (Exception ex)
						{
							throw new Exception("Invalid Session,session could no be parse!");
							

						}
					}
					SqlDataReader sqlReader = sqlCommand.ExecuteReader();
					if (sqlReader.Read())
					{
						session = Convert.ToInt32(sqlReader[0]);
						if (session > 0)
						{
							returnsessionData = new SessionResponseData();
							returnsessionData.UserID = sqlReader.GetInt32(1);
							returnsessionData.Session = encryptor.Encrypt(session.ToString());
						}
					}
				}
				if (returnsessionData == null)
					throw new Exception("User Name/Password is wrong!");
			}
			catch (Exception ex)
			{

				throw new Exception(ex.Message);
			}

			return returnsessionData;
		}

		#endregion






	}
}
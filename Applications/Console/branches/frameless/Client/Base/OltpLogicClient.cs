using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using Easynet.Edge.UI.Data;
using Easynet.Edge.UI.Server;
using System.Configuration;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;
using System.Deployment.Application;

namespace Easynet.Edge.UI.Client
{
	public class OltpProxy: IDisposable
	{
		#region Static
		//=========================

		static string _currentSessionID;
		static Oltp.UserRow _currentUser = null;
		static ServiceClient<IOltpLogic> _internalProxy = null;
		static string _serverAddress = null;

		static void InitProxy()
		{
			if (_internalProxy != null &&
				_internalProxy.State != System.ServiceModel.CommunicationState.Closed &&
				_internalProxy.State != System.ServiceModel.CommunicationState.Faulted)
			{
				throw new InvalidOperationException("A session is already open.");
			}

			_internalProxy = new ServiceClient<IOltpLogic>("IOltpLogic_Endpoint", ServerAddress);
		}

		public static string ServerAddress
		{
			get
			{
				if (_serverAddress == null)
				{
					if (!ApplicationDeployment.IsNetworkDeployed)
					{
						string serverAddressAbsolute = AppSettings.Get(typeof(OltpProxy), "ServerAddress.Absolute");
						_serverAddress = serverAddressAbsolute;
					}
					else
					{
						string serverAddressRelative = AppSettings.Get(typeof(OltpProxy), "ServerAddress.Relative");
						_serverAddress = new Uri(ApplicationDeployment.CurrentDeployment.ActivationUri, serverAddressRelative).ToString();
					}
				}

				return _serverAddress;
			}
		}

		public static void SessionStart(string sessionID)
		{
			InitProxy();
			_currentUser = (Oltp.UserRow) _internalProxy.Service.User_LoginBySessionID(sessionID).Rows[0];
			_currentSessionID = sessionID;
		}


		public static void SessionEnd()
		{
			if (_internalProxy != null && _internalProxy.State == System.ServiceModel.CommunicationState.Opened)
			{
				using (_internalProxy)
					_internalProxy.Close();
			}
		}

		public static Oltp.UserRow CurrentUser
		{
			get { return _currentUser; }
		}

		public static string CurrentSessionID
		{
			get { return _currentSessionID; }
		}


		//=========================
		#endregion

		#region Instance
		//=========================

		public OltpProxy()
		{
			if (_internalProxy == null || _internalProxy.State != System.ServiceModel.CommunicationState.Opened)
			{
				if (CurrentUser == null)
				{
					throw new System.Security.Authentication.AuthenticationException("You must be logged in to perform this operation.");
				}
				else
				{
					// Automatically restart session
					SessionStart(CurrentSessionID);
				}
			}
		}

		public IOltpLogic Service
		{
			get { return _internalProxy.Service; }
		}

		void IDisposable.Dispose()
		{
		}

		//=========================
		#endregion
	}
}

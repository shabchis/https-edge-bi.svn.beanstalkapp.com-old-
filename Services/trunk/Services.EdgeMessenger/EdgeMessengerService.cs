using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Messaging;


namespace Easynet.Edge.Services.Messenger
{
    public class EdgeMessengerService : Service, IMessengerService
    {
        private List<SocketChatClient> _clients = new List<SocketChatClient>();

        protected override ServiceOutcome DoWork()
        {
            //Open a socket, and wait for events.
            int listenPort = Convert.ToInt32(Instance.Configuration.Options["ListenPort"]);

            IPAddress[] aryLocalAddr = null;
            String strHostName = "";
            try
            {
                // NOTE: DNS lookups are nice and all but quite time consuming.
                strHostName = Dns.GetHostName();
                IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
                aryLocalAddr = ipEntry.AddressList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error trying to get local address {0} ", ex.Message);
                return ServiceOutcome.Failure;
            }

            // Verify we got an IP address. Tell the user if we did
            if (aryLocalAddr == null || aryLocalAddr.Length < 1)
            {
                Console.WriteLine("Unable to get local address");
                return ServiceOutcome.Failure;
            }
            //Console.WriteLine("Listening on : [{0}] {1}:{2}", strHostName, aryLocalAddr[0], listenPort);

            // Create the listener socket in this machines IP address
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Make sure to listen on the first "real" ip address we find (make sure it's not a MAC address...)
            IPAddress ip = null;
            for (int i = 0; i < aryLocalAddr.Length; i++)
            {
                ip = aryLocalAddr[i];
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    break;
            }

            listener.Bind(new IPEndPoint(ip, listenPort));
            listener.Listen(10);

            // Setup a callback to be notified of connection requests
            listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);

            return ServiceOutcome.Unspecified;
        }


        /// <summary>
        /// Callback used when a client requests a connection. 
        /// Accpet the connection, adding it to our list and setup to 
        /// accept more connections.
        /// </summary>
        /// <param name="ar"></param>
        public void OnConnectRequest(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            NewConnection(listener.EndAccept(ar));
            listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);
        }


        /// <summary>
        /// Add the given connection to our list of clients
        /// Note we have a new friend
        /// Send a welcome to the new client
        /// Setup a callback to recieve data
        /// </summary>
        /// <param name="sockClient">Connection to keep</param>
        //public void NewConnection( TcpListener listener )
        public void NewConnection(Socket sockClient)
        {
            // Program blocks on Accept() until a client connects.
            //SocketChatClient client = new SocketChatClient( listener.AcceptSocket() );
            SocketChatClient client = new SocketChatClient(sockClient);
            _clients.Add(client);
            //Console.WriteLine("Client {0}, joined", client.Sock.RemoteEndPoint);
        }



        #region IMessengerService Members

        public void Send(string recipient, string msg)
        {
            //When someone calls us, loop on all open connections, and based
            //on who the message is for, send it.
            foreach (SocketChatClient scc in _clients)
            {
                scc.Sock.Send(Encoding.Unicode.GetBytes(msg));
            }
        }

        #endregion
    }




    /// <summary>
    /// Class holding information and buffers for the Client socket connection
    /// </summary>
    internal class SocketChatClient
    {
        private Socket m_sock;						// Connection to the client
        private byte[] m_byBuff = new byte[50];		// Receive data buffer
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sock">client socket conneciton this object represents</param>
        public SocketChatClient(Socket sock)
        {
            m_sock = sock;
        }

        // Readonly access
        public Socket Sock
        {
            get { return m_sock; }
        }

    }

}

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketServer
{
	class Host
	{
		public static Random r = new();

		private static List<Socket> _clientSockets = new();

		public static Socket _socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		private static byte[] _buffer = new byte[1024];

		public static void Main()
		{
			Console.Title = $"Server {_socketServer.AddressFamily}";

			SetupServer();
			Console.ReadLine();
		}

		public static void SetupServer()
		{
			Console.WriteLine("Setting up the server...");
			_socketServer.Bind(new IPEndPoint(IPAddress.Any, 100));
			_socketServer.Listen(5);


			_socketServer.BeginAccept(new AsyncCallback(AcceptCallback), null);
		}

		public static void AcceptCallback(IAsyncResult AR)
		{
			Socket socket = _socketServer.EndAccept(AR);

			_clientSockets.Add(socket);
			Console.WriteLine("Client connected.");

			socket.BeginReceive(_buffer, 0, _buffer.Length,SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);

			_socketServer.BeginAccept(new AsyncCallback(AcceptCallback), null);

		}

		private static void ReceiveCallback(IAsyncResult AR)
		{
			Console.WriteLine("Request received!");
			Socket socket = (Socket)AR.AsyncState;
			int received = socket.EndReceive(AR);

			byte[] dataBuf = new byte[received];
			Array.Copy(_buffer, dataBuf, received);

			string textReceived = Encoding.ASCII.GetString(dataBuf);
			Console.WriteLine($"Text received: {textReceived}");

			string response = string.Empty;

			switch (textReceived.ToLower())
			{
				case string s when
					s.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries).Length == 3 &&
					s.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries)[0] == "rng":

					int min = int.Parse(s.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries)[1]);
                    int max = int.Parse(s.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries)[2]);

					response = $"Your generated number was {r.Next(min,max)}";
                    break;

				case string s when s.StartsWith("get time"):
					response = DateTime.Now.ToLongDateString();
					break;

				default:
					response = "Invalid request.";
					break;
			}

			byte[] data = Encoding.ASCII.GetBytes(response);
			socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
			_socketServer.BeginAccept(new AsyncCallback(AcceptCallback), null);

		}

		private static void SendCallback(IAsyncResult AR)
		{
			Console.WriteLine("Sending a callback...");
			Socket socket = (Socket)AR.AsyncState;
			socket.EndSend(AR);
		}
	}
}
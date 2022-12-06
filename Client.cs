using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketClient
{
    class Client
    {
        private static readonly Socket ClientSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private const int PORT = 100; //server is running on port 100

        static void Main()
        {
            Console.Title = "Client";
            InitialConnectLoop();
            RequestLoop();
            Exit();
        }
        /// <summary>
        /// Attempt to connect to the server.
        /// </summary>
        private static void InitialConnectLoop()
        {
            int attempts = 0;

            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine("Connection attempt " + attempts);
                    ClientSocket.Connect(IPAddress.Loopback, PORT);
                }
                catch (SocketException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message + "\n"); //print the exception out to the console
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nConnected.\n"); //connection succeeded
        }
        /// <summary>
        /// Loop receiving requests and keep the program alive.
        /// </summary>
        private static void RequestLoop()
        {
            while (true)
            {
                SendRequest();
                ReceiveResponse();
            }
        }

        /// <summary>
        /// Close socket and exit program.
        /// </summary>
        private static void Exit()
        {
            SendString("exit"); 
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Environment.Exit(0); //exit code 0 - OK
        }

        private static void SendRequest()
        {
            Console.Write(">");
            string request = Console.ReadLine();

            SendString(request);
            

            if (request.ToLower() == "exit")
            {
                Exit();
            }
        }

        /// <summary>
        /// Send an encoded ASCII string to the server.
        /// </summary>
        private static void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static void ReceiveResponse()
        {
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            Console.WriteLine(text);
        }
    }
}
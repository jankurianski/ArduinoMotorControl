using System;
using System.Net.Sockets;
using System.Net;

namespace ArduinoMotorServer
{
	class MainClass
	{
		static Socket socket;

		public static void Main(string[] args)
		{
			socket = new Socket(AddressFamily.InterNetwork, 
				SocketType.Stream, ProtocolType.Tcp);

			IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress ipAddress = ipHostInfo.AddressList[0];
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

			socket.Bind(localEndPoint);
			socket.Listen(1); // only allow 1 connection

			Console.WriteLine("Waiting for robot client to connect on " + ipAddress);

			Socket handler = socket.Accept();

			Console.WriteLine("W A S D to drive the robot");
			Console.WriteLine("Press the Escape (Esc) key to quit: \n");

			ConsoleKeyInfo cki;
			byte[] buffer = new byte[1];
			do {
				cki = Console.ReadKey();
				char cmd;
				switch (cki.Key) {
				case ConsoleKey.W:
					cmd = 'W';
					break;
				case ConsoleKey.A:
					cmd = 'A';
					break;
				case ConsoleKey.S:
					cmd = 'S';
					break;
				case ConsoleKey.D:
					cmd = 'D';
					break;
				default:
					cmd = 'Q';
					break;
				}
				buffer[0] = (byte)(int)cmd;
				handler.Send(buffer);
			} while (cki.Key != ConsoleKey.Escape);

			handler.Shutdown(SocketShutdown.Both);
			handler.Close();
		}
	}
}
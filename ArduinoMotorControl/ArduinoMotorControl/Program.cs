using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace MotorKeyControl
{
	/// <summary>
	/// A console application that is meant to be run on a computer
	/// that is connected to the Arduino via USB.
	/// 
	/// Only tested on Mac OSX.
	/// 
	/// Two modes:
	///   Keyboard: shows a console where you can use W A S D to drive
	///             the robot.
    ///   Socket:   connects to a server to listen for W A S D commands.
	/// </summary>
	class MainClass
	{
		static byte[] buffer = new byte[1];
		static SerialPort port;
        static Mode mode = Mode.Keyboard;
		static ICommandReceiver receiver;
		static bool test = false;

		public static void Main(string[] args) 
		{
			switch (mode) {
			case Mode.Keyboard:
				receiver = new KeyboardCommandReceiver();
				break;
			case Mode.Socket:
				receiver = new SocketCommandReceiver();
				break;
			default:
				Console.WriteLine("Unknown mode: " + mode.ToString ());
				Environment.ExitCode = 1;
				return;
			}

			if (!test) {
				port = new SerialPort ("/dev/tty.usbmodem1421", 9600);
				port.Open ();
			}

			try {
				CancellationTokenSource cancelStop = null;
				char? lastCmd = null;
				do {
					char cmd = receiver.BlockUntilNextCmd();

					if (cancelStop != null)
						cancelStop.Cancel();

					if (lastCmd == null
						|| lastCmd.Value != cmd) {
						Send(cmd);
					} 

					lastCmd = cmd;

					cancelStop = new CancellationTokenSource();
					Task.Delay(TimeSpan.FromMilliseconds(750), cancelStop.Token)
						.ContinueWith(task => {
							if (!task.IsCanceled) {
								cancelStop = null;
								lastCmd = null;
								Send('Q');
							}
						});
				} while (receiver.AnotherCommand());

				Send('Q');
			} finally {
				port.Close();
			}
		}

		static void Send(char c)
		{
			if (test)
				Console.WriteLine("Send({0})", c);
			else {
				buffer[0] = (byte)(int)c;
				try {
					port.Write(buffer, 0, 1);
				} catch (Exception e) {
					Console.WriteLine ("Send('{0}'): {1}", c, e.Message);
				}
			}
		}
	}

	interface ICommandReceiver
    {
		char BlockUntilNextCmd();
		bool AnotherCommand();
	}

	class KeyboardCommandReceiver : ICommandReceiver
    {
		ConsoleKeyInfo cki;

		public KeyboardCommandReceiver()
		{
			Console.WriteLine("W A S D to drive the robot");
			Console.WriteLine("Press the Escape (Esc) key to quit: \n");
		}

		public char BlockUntilNextCmd()
		{
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
			return cmd;
		}

		public bool AnotherCommand() {
			return cki.Key != ConsoleKey.Escape;
		}
	}

	class SocketCommandReceiver : ICommandReceiver
    {
		Socket socket;
		byte[] buffer = new byte[1];

		public SocketCommandReceiver()
		{
			this.socket = new Socket(AddressFamily.InterNetwork, 
				SocketType.Stream, ProtocolType.Tcp);

			IPAddress ipAddress = IPAddress.Parse("192.168.1.101");
			IPEndPoint ipe = new IPEndPoint(ipAddress,11000);
			try {
				socket.Connect(ipe);
			} catch(Exception e) {
				Console.WriteLine("Unexpected exception : {0}", e.ToString());
				throw;
			}
		}

		public char BlockUntilNextCmd()
        {
            char cmd = 'Q';

            int i = socket.Receive(buffer, 0, 1, SocketFlags.None);
            if (i > 0)
                cmd = (char)(int)buffer[0];

            return cmd;
        }

		public bool AnotherCommand()
		{
			return socket.Connected;
		}
	}

	enum Mode
    {
		Keyboard,
		Socket
	}
}
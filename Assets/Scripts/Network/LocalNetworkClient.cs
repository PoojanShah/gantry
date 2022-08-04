using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Network
{
	public class LocalNetworkClient
	{
		public static event Action<int> OnMediaAmountReceived;

		private static bool _isLoaded = false;

		public static void SendPlayMessage(int ipLastNumber, int videoId)
		{
			Debug.Log("start client");

			var receivedData = new byte[NetworkHelper.BUFFER_SIZE];

			try
			{
				var ipAddress = IPAddress.Parse(NetworkHelper.GetMyIpWithoutLastNumberString() + ipLastNumber);
				var remoteEP = new IPEndPoint(ipAddress, NetworkHelper.PORT);
				
				Debug.Log("Connecting to + " + remoteEP);
				
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				try
				{
					socket.Connect(remoteEP);

					var messageToSend = Encoding.ASCII.GetBytes(NetworkHelper.NETWORK_MESSAGE_PREFIX + videoId);

					socket.Send(messageToSend);

					var bytesRec = socket.Receive(receivedData);
					var mediaAmount = int.Parse(Encoding.ASCII.GetString(receivedData, 0, bytesRec));

					if (!_isLoaded)
					{
						Debug.Log(mediaAmount);

						OnMediaAmountReceived?.Invoke(mediaAmount);

						_isLoaded = true;
					}

					socket.Shutdown(SocketShutdown.Both);
					socket.Close();
				}
				catch (ArgumentNullException ane)
				{
					Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
				}
				catch (SocketException se)
				{
					Console.WriteLine("SocketException : {0}", se.ToString());
				}
				catch (Exception e)
				{
					Console.WriteLine("Unexpected exception : {0}", e.ToString());
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}
	}
}

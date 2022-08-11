using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Core;
using Screens;
using UnityEngine;

namespace Network
{
	public class LocalNetworkClient
	{
		public static event Action<int> OnMediaAmountReceived;

		private static bool _isLoaded = false;
		private static MainMenuAndroid _menu;

		public LocalNetworkClient(MainMenuAndroid menu)
		{
			_menu = menu;
		}

		public static void SendPlayMessage(int ipLastNumber, int videoId)
		{
			Debug.Log("start client");

			var bytesBuffer = new byte[NetworkHelper.BUFFER_SIZE];

			try
			{
				var ipAddress = IPAddress.Parse(NetworkHelper.GetMyIpWithoutLastNumberString() + ipLastNumber);
				var remoteEP = new IPEndPoint(ipAddress, NetworkHelper.PORT);
				
				Debug.Log("Connecting to + " + remoteEP);
				
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				try
				{
					socket.Connect(remoteEP);

					var messageToSend = Encoding.ASCII.GetBytes(NetworkHelper.NETWORK_MESSAGE_PLAY_PREFIX + videoId);

					socket.Send(messageToSend);

					var receivedBytes = socket.Receive(bytesBuffer);

					void HandleReceivedMessage()
					{
						var receivedData = Encoding.ASCII.GetString(bytesBuffer, 0, receivedBytes);

						if (int.TryParse(receivedData, out var result))
						{
							if (!_isLoaded)
							{
								OnMediaAmountReceived?.Invoke(result);

								_isLoaded = true;
							}

							return;
						}

						var videoData = receivedData.Split(Constants.Underscore);

						if (videoData.Length == NetworkHelper.VIDEO_DATA_AMOUNT)
						{
							_menu.UpdateMediaTitle(int.Parse(videoData[2]), videoData[1]);
						}
					}

					HandleReceivedMessage();

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

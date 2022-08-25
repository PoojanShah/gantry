using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Core;
using UnityEngine;

namespace Network
{
	public class LocalNetworkClient
	{
		public static event Action<Dictionary<int, string>> OnMediaInfoReceived;

		private static int _ipLastNumber = -1;

		public static void Connect(int ipLastNumber)
		{
			_ipLastNumber = ipLastNumber;

			SendPlayMessage(-1); //init connection
		}

		public static void SendPlayMessage(int videoId) =>
			SendMessage(NetworkHelper.NETWORK_MESSAGE_PLAY_PREFIX + videoId);

		public static void SendMuteMessage() => SendMessage(NetworkHelper.NETWORK_MESSAGE_MUTE);

		public static void SendMessage(string message)
		{
			Debug.Log("start client");

			var bytesBuffer = new byte[NetworkHelper.BUFFER_SIZE];

			try
			{
				var ipFirstPart = NetworkHelper.GetMyIpWithoutLastNumberString();
				var ipAddress = IPAddress.Parse(ipFirstPart + _ipLastNumber);
				var remoteEP = new IPEndPoint(ipAddress, NetworkHelper.PORT);

				Debug.Log("Connecting to + " + remoteEP);

				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				try
				{
					socket.Connect(remoteEP);

					var messageToSend = Encoding.ASCII.GetBytes(message);

					socket.Send(messageToSend);

					var receivedBytes = socket.Receive(bytesBuffer);

					void HandleReceivedMessage()
					{
						var receivedData = Encoding.ASCII.GetString(bytesBuffer, 0, receivedBytes);
						//{amount of media}_{media title}:{media id}_..._{media title}:{media id}
						var parsedData = receivedData.Split(Constants.Underscore);

						int.TryParse(parsedData[0], out var mediaAmount);

						var mediaDictionary = new Dictionary<int, string>(mediaAmount);

						for (var i = 1; i < parsedData.Length; i++)
						{
							var videoData = parsedData[i].Split(Constants.DoubleDot);

							mediaDictionary.Add(int.Parse(videoData[1]), videoData[0]);
						}

						OnMediaInfoReceived?.Invoke(mediaDictionary);
					}

					HandleReceivedMessage();
					
					NetworkHelper.SaveIP(ipFirstPart, _ipLastNumber);
					
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Core;
using UnityEngine;

namespace Network
{
	public class LocalNetworkClient
	{
		public static event Action<Dictionary<int, string>> OnMediaInfoReceived;
		public static event Action<Texture2D> OnThumbnailReceived;

		private static readonly ConcurrentQueue<byte[]> ImagesQueue = new();

		private static TcpClient _tcpClient;
		private static Thread _networkThread;
		private static bool _isNetworkRunning, _isMediaDataReceived, _isThumbnailsReceived;

		private static int _ipLastNumber = -1, _mediaLength = -1;

		public static void Connect(int ipLastNumber)
		{
			_ipLastNumber = ipLastNumber;

			_isNetworkRunning = true;
			_networkThread = new Thread(NetworkThread);
			_networkThread.Start();
		}

		public void Clear()
		{
			_isNetworkRunning = false;

			if (_networkThread == null)
				return;

			const int millisecondsTimeout = 100;

			if (!_networkThread.Join(millisecondsTimeout))
				_networkThread.Abort();
		}

		private static void NetworkThread()
		{
			_tcpClient = new TcpClient();
			var ipFirstPart = NetworkHelper.GetMyIpWithoutLastNumberString();
			var ipAddressParsed = IPAddress.Parse(ipFirstPart + _ipLastNumber);

			_tcpClient.Connect(ipAddressParsed, NetworkHelper.PORT);

			using var stream = _tcpClient.GetStream();

			var reader = new BinaryReader(stream);

			while (_isNetworkRunning && _tcpClient.Connected && stream.CanRead)
			{
				var length = reader.ReadInt32();
				var receivedBytes = reader.ReadBytes(length);

				try
				{
					if (!_isMediaDataReceived)
					{
						var message = Encoding.ASCII.GetString(receivedBytes);

						var parsedData = message.Split(Constants.Underscore);
						//{amount of media}_{media title}:{media id}_..._{media title}:{media id}

						int.TryParse(parsedData[0], out var mediaAmount);

						var mediaDictionary = new Dictionary<int, string>(mediaAmount);

						for (var i = 1; i < parsedData.Length; i++)
						{
							var videoData = parsedData[i].Split(Constants.DoubleDot);

							mediaDictionary.Add(int.Parse(videoData[1]), videoData[0]);
						}

						_isMediaDataReceived = true;

						UnityMainThreadDispatcher.Instance().Enqueue(() =>
						{
							//SaveIp();

							OnMediaInfoReceived?.Invoke(mediaDictionary);
						});
					}
					else if (!_isThumbnailsReceived)
					{
						ImagesQueue.Enqueue(receivedBytes);

						UnityMainThreadDispatcher.Instance().Enqueue(() =>
						{
							const int defaultTextureSize = 1;
							var texture = new Texture2D(defaultTextureSize, defaultTextureSize);

							if (ImagesQueue.Count > 0 && ImagesQueue.TryDequeue(out var data))
							{
								texture.LoadImage(data);
								texture.Apply();
							}

							OnThumbnailReceived?.Invoke(texture);
						});
					}
				}
				catch (Exception e)
				{
					Debug.Log(e);
				}
				finally
				{
					//UnityMainThreadDispatcher.Instance().Enqueue(() =>
					//{
					//	Debug.Log("ip saved");
					//	SaveIp();
					//});
				}
			}
		}

		public static void SendPlayMessage(int videoId) =>
			SendMessage(NetworkHelper.NETWORK_MESSAGE_PLAY_PREFIX + videoId);

		public static void SendMuteMessage() => SendMessage(NetworkHelper.NETWORK_MESSAGE_MUTE);

		public static void SendMessage(string message)
		{ 
			Debug.Log("sending message: " + message);
			var bytes = Encoding.ASCII.GetBytes(message);
			var writer = new BinaryWriter(_tcpClient.GetStream());
			if (!_tcpClient.Connected)
				return;

			writer.Write(bytes);
		}

		private static void SaveIp()
		{
			var ipFirstPart = NetworkHelper.GetMyIpWithoutLastNumberString();

			NetworkHelper.SaveIP(ipFirstPart, _ipLastNumber);
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Core;
using Media;
using Screens;
using Subscription;
using UnityEngine;

namespace Network
{
	public class FileItem
	{
		public string name;
		public byte[] data;
	}

	public class ConnectedClient
	{
		public TcpClient client;
		public BinaryWriter writer;

		public ConnectedClient(TcpClient aClient)
		{
			client = aClient;
			writer = new BinaryWriter(client.GetStream());
			new BinaryReader(client.GetStream());
		}

		public bool SendImageData(byte[] aData)
		{
			if (!client.Connected)
				return false;

			writer.Write(aData.Length);
			writer.Write(aData);
			return true;
		}

		public bool SendMediaData(byte[] mediaData)
		{
			if (!client.Connected)
				return false;

			writer.Write(mediaData.Length);
			writer.Write(mediaData);
			return true;
		}
	}

	public class LocalNetworkServer
	{
		private static MediaController _mediaController;
		private static OptionsSettings _settings;

		public static int ReceivedId = -1;

		private static TcpListener _server;
		private static bool _isServerRunning, _isSending, _isMediaDataSent;
		private static Thread _listenerThread, _sendingThread;
		private static readonly List<ConnectedClient> _clients = new();

		public LocalNetworkServer(MediaController mediaController, OptionsSettings settings)
		{
			_mediaController = mediaController;
			_settings = settings;

			_mediaController.OnMediaChanged += UpdateMedia;

			SetupServer();
		}

		private static void UpdateMedia()
		{
			Debug.Log("media updated, need to update clients");

			_isMediaDataSent = false;

			SendMediaData();
			SendThumbnails();
		}

		public void Clear()
		{
			lock (_clients)
			{
				foreach (var c in _clients)
				{
					try
					{
						c.client.Close();
					}
					catch
					{
						// ignored
					}
				}

				_clients.Clear();
			}

			_mediaController.OnMediaChanged -= UpdateMedia;

			_isSending = false;
			_isServerRunning = false;
			_server?.Stop();
		}

		private static void SetupServer()
		{
			Debug.Log("Setting up server...");

			var myIpAddress = NetworkHelper.GetMyIp();

			if(myIpAddress == null)
				return;

			_listenerThread = new Thread(ListenThread);
			_listenerThread.Start();

			_sendingThread = new Thread(SendThread);
			_sendingThread.Start();
		}

		private static void ListenThread()
		{
			_server = new TcpListener(NetworkHelper.GetMyIp(), NetworkHelper.PORT);
			_server.Start();
			_isServerRunning = true;

			while (_isServerRunning)
			{
				try
				{
					var newClient = _server.AcceptTcpClient();

					lock (_clients)
					{
						_clients.Add(new ConnectedClient(newClient));

						_isMediaDataSent = false; //need to refresh

						SendMediaData();

						SendThumbnails();
					}
				}
				catch (Exception e)
				{
					Debug.Log(e.Message);
				}

				try
				{
					foreach (var connectedClient in _clients)
					{
						if (!SubscriptionController.IsSubscriptionActive)
							return;

						var stream = connectedClient.client.GetStream();

						byte[] bytes = new byte[NetworkHelper.BUFFER_SIZE];
						int recv = 0;

						while (true)
						{
							recv = stream.Read(bytes, 0, NetworkHelper.BUFFER_SIZE);
							var received = Encoding.ASCII.GetString(bytes, 0, recv);

							if (string.IsNullOrEmpty(received))
							{
								Debug.Log("received empty message. closing client...");

								lock (_clients)
								{
									foreach (var c in _clients)
									{
										try
										{
											c.client.Close();
											Debug.Log("close");
										}
										catch
										{
										}
									}

									_clients.Clear();
								}

								break;
							}

							Debug.Log("received message: " + received);

							if (string.IsNullOrEmpty(received) || !received.Contains(Constants.Underscore))
								continue;

							UnityMainThreadDispatcher.Instance().Enqueue(() =>
							{
								HandleReceivedMessage(received);
							});
						}
					}
				}
				catch (Exception e)
				{
					Debug.Log(e.Message);
				}
			}
		}

		private static void SendThread()
		{
			_isSending = true;
		}

		private static void SendThumbnails()
		{
			if (_clients?.Count == 0)
				return;

			var folder = new DirectoryInfo(Settings.ThumbnailsPath);
			var fileNames = folder.GetFiles("*.png");
			var files = fileNames
				.Select(fn => new FileItem { data = File.ReadAllBytes(fn.FullName), name = fn.FullName }).ToList();

			var fileId = 0;

			const int millisecondsTimeout = 300;

			ConnectedClient[] clients;

			while (_isSending && fileId < files.Count)
			{
				Thread.Sleep(millisecondsTimeout);

				var file = files[fileId];

				lock (_clients)
					clients = _clients.ToArray();

				foreach (var client in clients)
				{
					var success = false;

					try
					{
						success = client.SendImageData(file.data);

						fileId++;
					}
					catch
					{
						success = false;

						client.client.Close();
					}
					finally
					{
						if (!success)
						{
							lock (_clients)
							{
								_clients.Remove(client);
							}
						}
					}
				}
			}
		}

		private static void SendMediaData()
		{
			ConnectedClient[] clients;

			if(_clients?.Count == 0)
				return;

			while (_isSending && !_isMediaDataSent)
			{
				lock (_clients)
					clients = _clients.ToArray();

				foreach (var client in clients)
				{
					var success = false;

					try
					{
						var message = _mediaController.MediaFiles.Length.ToString();
						message = AddMediaInfo(
							message); //{amount of media}_{media title}:{media id}_..._{media title}:{media id}

						var data = Encoding.ASCII.GetBytes(message);

						success = client.SendMediaData(data);
					}
					catch
					{
						success = false;

						client.client.Close();
					}
					finally
					{
						if (!success)
						{
							lock (_clients)
							{
								_clients.Remove(client);
							}
						}
						else
							_isMediaDataSent = true;
					}
				}
			}
		}

		private static void HandleReceivedMessage(string text)
		{
			if (text == NetworkHelper.NETWORK_MESSAGE_MUTE)
				_settings.SwitchSound();
			else if (int.TryParse(text.Split(Constants.Underscore)[1], out var mediaId))
			{
				if (mediaId < 0)
					return;

				ReceivedId = mediaId;
			}
		}

		private static string AddMediaInfo(string message)
		{
			foreach (var media in _mediaController.MediaFiles)
			{
				var part = string.Format(NetworkHelper.NETWORK_MESSAGE_INFO_FORMAT, media.Name, media.Id);

				message += part;
			}

			return message;
		}
	}
}

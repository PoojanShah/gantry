using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Core;
using Media;
using UnityEngine;

namespace Network
{
	[Serializable]
	public struct SocketStruct
	{
		public string id;
		public Socket socket;
	}

	public class LocalNetworkServer
	{
		private static readonly Socket _serverSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private static readonly List<Socket> _clientSockets = new();
		private static readonly List<SocketStruct> _clientSocketStructList = new();

		private static readonly byte[] _buffer = new byte[NetworkHelper.BUFFER_SIZE];
		private static MediaController _mediaController;

		public static int ReceivedId = -1;

		public LocalNetworkServer(MediaController mediaController)
		{
			_mediaController = mediaController;

			SetupServer();
		}

		public void Clear() => CloseAllSockets();

		private static void SetupServer()
		{
			Debug.Log("Setting up server...");

			var myIpAddress = NetworkHelper.GetMyIp();
			var endPoint = new IPEndPoint(myIpAddress, NetworkHelper.PORT);

			_serverSocket.Bind(endPoint);
			_serverSocket.Listen(0);
			_serverSocket.BeginAccept(AcceptCallback, null);

			Debug.Log("Server setup complete on address: " + _serverSocket.LocalEndPoint);
		}

		private static void CloseAllSockets()
		{
			foreach (var socket in _clientSockets)
			{
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
			}

			_serverSocket.Close();
		}

		private static void AcceptCallback(IAsyncResult AR)
		{
			Socket socket;

			try {
				socket = _serverSocket.EndAccept(AR);
			}
			catch (ObjectDisposedException)
			{
				return;
			}

			_clientSockets.Add(socket);
			socket.BeginReceive(_buffer, 0, NetworkHelper.BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
			Debug.Log("Client connected, waiting for request...");

			var data = Encoding.ASCII.GetBytes(_mediaController.MediaFiles.Length.ToString());
			socket.Send(data);

			SendMediaData(socket);

			_serverSocket.BeginAccept(AcceptCallback, null);
		}

		private static void ReceiveCallback(IAsyncResult AR)
		{
			Socket current = (Socket)AR.AsyncState;
			int received;

			try {
				received = current.EndReceive(AR);
			}
			catch (SocketException)
			{
				Debug.Log("Client forcefully disconnected: ");
				SetUnRegister(current);
				current.Close();
				_clientSockets.Remove(current);
				return;
			}

			if (received <= 0)
			{
				Debug.Log("Client forcefully disconnected: ");
				SetUnRegister(current);
				current.Close();
				_clientSockets.Remove(current);
				return;
			}

			byte[] recBuf = new byte[received];
			Array.Copy(_buffer, recBuf, received);
			string text = Encoding.ASCII.GetString(recBuf);
			Debug.Log("Server Received Text: " + text);

			SetRegister(current, text);

			current.BeginReceive(_buffer, 0, NetworkHelper.BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);

			var mediaId = int.Parse(text.Split(Constants.Underscore)[1]);

			if (mediaId < 0)
				return;

			ReceivedId = mediaId;
		}

		private static void SendMediaData(Socket socket)
		{
			foreach (var media in _mediaController.MediaFiles)
			{
				var data = Encoding.ASCII.GetBytes(
					string.Format(NetworkHelper.NETWORK_MESSAGE_INFO_FORMAT, media.Name, media.Id));

				socket.Send(data);
			}
		}

		private static void SetRegister(Socket socket, string text)
		{
			string[] subStrings = text.Split(Constants.Coma);
			if (subStrings[0].Contains("register"))
			{
				SocketStruct cs = new SocketStruct();
				cs.socket = socket;
				cs.id = subStrings[1];
				_clientSocketStructList.Add(cs);
			}

			if (text.Contains("exit"))
			{
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
				_clientSockets.Remove(socket);
				Debug.Log("Client forcefully disconnected: ");
			}
		}

		private static void SetUnRegister(Socket socket)
		{
			SocketStruct cl = _clientSocketStructList.Find(c => c.socket == socket);
			_clientSocketStructList.Remove(cl);
		}
	}
}

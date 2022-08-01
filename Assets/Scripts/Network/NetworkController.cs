using UnityEngine;

namespace Network
{
	public class NetworkController
	{
		public const string IP = "192.168.1.114";
		public const int PORT = 8888;

		private TCPTestClient _client;
		private TCPTestServer _server;

		public NetworkController()
		{
			return;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			_server = new TCPTestServer();
#elif UNITY_ANDROID && !UNITY_EDITOR
			Debug.Log("Android");
			_client = new TCPTestClient();
#endif
		}

		public void SendMessage()
		{
			return;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			_server.SendMessage();
#elif UNITY_ANDROID && !UNITY_EDITOR
			_client.SendMessage();
			Debug.Log("Android");
#endif
		}
	}
}

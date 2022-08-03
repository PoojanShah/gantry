using System;

namespace Network
{
	public class NetworkController 
	{
		private readonly LocalNetworkServer _server;
		private LocalNetworkClient _client;

#if UNITY_STANDALONE_WIN
		public NetworkController(Action<int> playById)
		{
			_server = new LocalNetworkServer(playById);
		}
#elif UNITY_ANDROID
		public NetworkController()
		{
			_client = new LocalNetworkClient();
		}
#endif

		public void Clear()
		{
#if UNITY_STANDALONE_WIN
			_server.Clear();
#endif
		}
	}
}

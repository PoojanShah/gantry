using System;

namespace Network
{
	public class NetworkController 
	{
		private readonly LocalNetworkServer _server;
		private CustomNetworkClient _client;

		public NetworkController(Action<int> playById)
		{
#if UNITY_STANDALONE_WIN
			_server = new LocalNetworkServer(playById);
#elif UNITY_ANDROID

#endif
		}

		public void Clear()
		{
#if UNITY_STANDALONE_WIN
			_server.Clear();
#elif UNITY_ANDROID

#endif
		}
	}
}

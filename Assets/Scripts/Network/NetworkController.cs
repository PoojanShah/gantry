using System;
using Media;
using Screens;

namespace Network
{
	public class NetworkController 
	{
		private readonly LocalNetworkServer _server;
		private LocalNetworkClient _client;

#if UNITY_STANDALONE_WIN
		public NetworkController(MediaController mediaController)
		{
			_server = new LocalNetworkServer(mediaController);
		}
#elif UNITY_ANDROID
		public NetworkController(MainMenuAndroid menu)
		{
			_client = new LocalNetworkClient(menu);
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

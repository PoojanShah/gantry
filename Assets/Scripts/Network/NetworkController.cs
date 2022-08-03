using System;
using Media;
using Screens;
using VideoPlaying;

namespace Network
{
	public class NetworkController 
	{
		private readonly LocalNetworkServer _server;
		private LocalNetworkClient _client;

#if UNITY_STANDALONE_WIN
		public NetworkController(ProjectionController projectionController, MediaController mediaController)
		{
			_server = new LocalNetworkServer(projectionController, mediaController);
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

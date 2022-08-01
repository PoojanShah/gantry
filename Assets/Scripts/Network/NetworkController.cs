using UnityEngine;

namespace Network
{
	public class NetworkController
	{
		public const string IP = "192.168.1.114";
		public const int PORT = 8888;


		public NetworkController()
		{
			return;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			
#elif UNITY_ANDROID && !UNITY_EDITOR
			
			
#endif
		}

		public void SendMessage()
		{
			return;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			
#elif UNITY_ANDROID && !UNITY_EDITOR
			
#endif
		}
	}
}

using System.Net;

namespace Network
{
	public static class NetworkHelper
	{
		public const int PORT = 8888;

		public static IPAddress GetMyIp()
		{
			var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

//#if UNITY_STANDALONE_WIN || UNITY_EDITOR
			return ipHostInfo.AddressList[1];
//#endif
		}
	}
}

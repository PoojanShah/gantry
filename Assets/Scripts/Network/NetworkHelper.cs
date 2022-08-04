using System.Net;

namespace Network
{
	public static class NetworkHelper
	{
		public const int BUFFER_SIZE = 512;
		public const int PORT = 8888;
		public const string NETWORK_MESSAGE_PREFIX = "Play_";

		public static IPAddress GetMyIp()
		{
			var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
			return ipHostInfo.AddressList[1];
#elif UNITY_ANDROID
			return ipHostInfo.AddressList[0];
#endif
		}

		public static string GetMyIpWithoutLastNumberString()
		{
			const char separator = '.';
			var ip = GetMyIp().ToString().Split(separator);

			var result = ip[0] + separator + ip[1] + separator + ip[2] + separator;

			return result;
		}
	}
}

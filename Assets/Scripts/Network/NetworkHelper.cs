using System.Net;
using System.Net.NetworkInformation;

namespace Network
{
	public static class NetworkHelper
	{
		public const int PORT = 8888;
		public const string NETWORK_MESSAGE_PREFIX = "Play_";
		public const string pingHost = "google.com";

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
		
		public static bool GetPing()
		{
			try
			{
				Ping P = new Ping();
				PingReply Status = P.Send(pingHost, 3);
				return (Status.Status == IPStatus.Success);
			}
			catch 
			{ 
			}

			return false;
		}
		
		public static bool FileExists(string url)
		{
			bool result = false;
 
			var request = WebRequest.Create(url);
			request.Timeout = 1200;
			request.Method = "HEAD";
 
			HttpWebResponse response = null;
 
			try
			{
				response = (HttpWebResponse)request.GetResponse();
				result = true;
			}
			catch (WebException webException)
			{
			}
			finally
			{
				response?.Close();
			}
 
 
			return result;
		}
	}
}

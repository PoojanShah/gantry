using System.Net;
using System.Net.NetworkInformation;

namespace Network
{
	public static class NetworkHelper
	{
		public const int PORT = 8888;
		public const string NETWORK_MESSAGE_PREFIX = "Play_";
		public const string PING_HOST = "google.com";
		public const string FILE_EXIST_REQUEST_METHOD = "HEAD";
		public const int FILE_EXIST_TIMEOUT = 1200;
		public const int PING_TIMEOUT = 500;

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
		
		public static bool IsConnectionAvailable()
		{
			try
			{
				var ping = new Ping();
				var status = ping.Send(PING_HOST, PING_TIMEOUT);
				return status.Status == IPStatus.Success;
			}
			catch 
			{ 
			}

			return false;
		}
		
		public static bool CheckIsFileExist(string url)
		{
			var result = false;
 
			var request = WebRequest.Create(url);
			request.Timeout = FILE_EXIST_TIMEOUT;
			request.Method = FILE_EXIST_REQUEST_METHOD;
 
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

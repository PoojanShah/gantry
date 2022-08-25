using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;
using Ping = System.Net.NetworkInformation.Ping;

namespace Network
{
	public static class NetworkHelper
	{
		public const int BUFFER_SIZE = 512;
		public const int PORT = 8888;
		public const string NETWORK_MESSAGE_PLAY_PREFIX = "Play_";
		public const string NETWORK_MESSAGE_MUTE = "Mute";
		public const string NETWORK_MESSAGE_INFO_FORMAT = "_{0}:{1}"; //0 - name, 1 - id
		public const string PING_HOST = "google.com";
		public const string FILE_EXIST_REQUEST_METHOD = "HEAD";
		public const int FILE_EXIST_TIMEOUT = 1200;
		public const int PING_TIMEOUT = 500;
		private const string FIRST_PART_IP_KEY = "firstPartIP";
		private const string SECOND_PART_IP_KEY = "secondPartIP";

		public static int LastIpNumber = -1;

		public static IPAddress GetMyIp()
		{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
			var ip = GetLocalIpAddress();

			if (IPAddress.TryParse(ip, out var parsed))
				return parsed;

			Debug.LogError("Can't get your local IP address. Android app will not be able to connect");

			return null;

#elif UNITY_ANDROID
			var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			
			return ipHostInfo.AddressList[0];
#endif
		}

		private static string GetLocalIpAddress()
		{
			foreach (var netI in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (netI.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
				    (netI.NetworkInterfaceType != NetworkInterfaceType.Ethernet ||
				     netI.OperationalStatus != OperationalStatus.Up)) continue;
				foreach (var uniIpAddrInfo in netI.GetIPProperties().UnicastAddresses.Where(x => netI.GetIPProperties().GatewayAddresses.Count > 0))
				{

					if (uniIpAddrInfo.Address.AddressFamily == AddressFamily.InterNetwork &&
					    uniIpAddrInfo.AddressPreferredLifetime != uint.MaxValue)
						return uniIpAddrInfo.Address.ToString();
				}
			}

			return null;
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
		
		public static bool IsFileExist(string url)
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

		public static void SaveIP(string ipFirstPart, int ipSecondPart)
		{
			PlayerPrefs.SetString(FIRST_PART_IP_KEY, ipFirstPart);
			PlayerPrefs.SetInt(SECOND_PART_IP_KEY, ipSecondPart);
			PlayerPrefs.Save();
		}

		public static bool IsSavedIpValid()
		{
			const int notInitializedIpNumber = -1;
			var savedIP = PlayerPrefs.GetString(FIRST_PART_IP_KEY, string.Empty);
			var savedLastPartOfIP = PlayerPrefs.GetInt(SECOND_PART_IP_KEY, notInitializedIpNumber);

			if (savedIP != GetMyIpWithoutLastNumberString() && savedLastPartOfIP < 0)
				return false;

			LastIpNumber = savedLastPartOfIP;

			return true;
		}
	}
}

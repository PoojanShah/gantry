using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class IPTest : MonoBehaviour
{
	private static TMP_Text _ip;

	private void Awake()
	{
		_ip = GetComponent<TMP_Text>();

		_ip.text = GetLocalIpAddress();
	}

	public static string GetLocalIpAddress()
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
		_ip.text += "You local IPv4 address couldn't be found...";

		return null;
	}
}

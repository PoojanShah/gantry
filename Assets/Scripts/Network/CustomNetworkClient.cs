using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Network;
using TMPro;
using UnityEngine;

public class CustomNetworkClient : MonoBehaviour
{
	public static void SendMessagePlay(int ipLastNumber, int videoId)
    {
        Debug.Log("start client");
        // Data buffer for incoming data.  
        byte[] bytes = new byte[1024];

        // Connect to a remote device.  
        try
        {
	        // Establish the remote endpoint for the socket.  

	        var ipAddress = IPAddress.Parse(NetworkHelper.GetMyIpWithoutLastNumberString() + ipLastNumber);
	        var remoteEP = new IPEndPoint(ipAddress, NetworkHelper.PORT);
	        Debug.Log("Connecting to + " + remoteEP);
	        // Create a TCP/IP  socket.  
	        var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

	        // Connect the socket to the remote endpoint. Catch any errors.  
	        try
	        {
		        sender.Connect(remoteEP);

		        // Encode the data string into a byte array.  
		        var msg = Encoding.ASCII.GetBytes(NetworkHelper.NETWORK_MESSAGE_PREFIX + videoId);

		        // Send the data through the socket.  
		        var bytesSent = sender.Send(msg);

		        // Receive the response from the remote device.  
		        //int bytesRec = sender.Receive(bytes);

		        //Debug.Log(Encoding.ASCII.GetString(bytes, 0, bytesRec));

		        // Release the socket.  
		        sender.Shutdown(SocketShutdown.Both);
		        sender.Close();
	        }
	        catch (ArgumentNullException ane)
	        {
		        Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
	        }
	        catch (SocketException se)
	        {
		        Console.WriteLine("SocketException : {0}", se.ToString());
	        }
	        catch (Exception e)
	        {
		        Console.WriteLine("Unexpected exception : {0}", e.ToString());
	        }

        }
        catch (Exception e)
        {
	        Console.WriteLine(e.ToString());
        }
    }
}

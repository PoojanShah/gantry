using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CustomNetworkClient : MonoBehaviour
{
	// Use this for initialization
    private void Start()
    {
	    StartClient();
    }

    void Update()
    {
	    if (Input.GetKeyDown(KeyCode.A))
		    StartClient();
    }

	public static void StartClient()
    {
        Debug.Log("start client");
        // Data buffer for incoming data.  
        byte[] bytes = new byte[1024];

        // Connect to a remote device.  
        try {
            // Establish the remote endpoint for the socket.  
            // This example uses port 11000 on the local computer.  
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            Debug.Log(ipHostInfo);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            Debug.Log(ipAddress);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);
            Debug.Log(remoteEP.ToString());
            Debug.Log("init remote EP");
            // Create a TCP/IP  socket.  
            Socket sender = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.  
            try {
	            Debug.Log(" sender.Connect()");
                sender.Connect(remoteEP);

                Debug.Log(sender.RemoteEndPoint.ToString());

                // Encode the data string into a byte array.  
                byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");
                
                // Send the data through the socket.  
                int bytesSent = sender.Send(msg);

                // Receive the response from the remote device.  
                int bytesRec = sender.Receive(bytes);

                //Debug.Log(Encoding.ASCII.GetString(bytes, 0, bytesRec));

                // Release the socket.  
                //sender.Shutdown(SocketShutdown.Send);
                //sender.Close();

            }
            catch (ArgumentNullException ane) {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se) {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e) {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        }
        catch (Exception e) {
            Console.WriteLine(e.ToString());
        }
    }
}

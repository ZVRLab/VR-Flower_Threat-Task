using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

 

public class UDPSender : MonoBehaviour

{
	
    private static int localPort;  

    // prefs 

    private string IP;  // define in init

    private int port;  // define in init

    
    // "connection" things

    public static IPEndPoint remoteEndPoint;

    public static UdpClient client;

    
    // call it from shell (as program)

    private static void Main() 

    {

        UDPSender sendObj=new UDPSender();

        sendObj.init();

        
    }

    // start from unity3d

    public void Start()

    {

        init(); 

    }

    // init

    public void init()

    {
		
		// define

		IP="127.0.0.1";

		port=8105;  

		// ----------------------------

		// Senden
		
		// ----------------------------

		remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);

		client = new UdpClient();

    }


    // sendData

    public static void sendString(string message)

    {
		//Debug.Log ("We are here!");
        try 

        {
			byte[] data = Encoding.UTF8.GetBytes(message);
			client.Send(data, data.Length, remoteEndPoint);

 
        }

        catch (Exception err)

        {

            print(err.ToString());

        }

    }

}
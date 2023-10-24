using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TucanFPSDS
{
public class Server
{
    private UdpClient udpClient;
    private IPEndPoint localEndPoint;
    private Thread receiveThread;
    private List<IPEndPoint> connectedClients;

    public Server()
    {
        connectedClients = new List<IPEndPoint>();
    }

    public void Start(int port)
    {
        udpClient = new UdpClient(port);
        localEndPoint = new IPEndPoint(IPAddress.Any, port);

        receiveThread = new Thread(new ThreadStart(ReceiveMessages));
        receiveThread.Start();
    }

    public void Stop()
    {
        udpClient.Close();
        receiveThread.Join();
    }

    public void SendMessage(string message, IPEndPoint clientEndPoint)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, clientEndPoint);
        Console.WriteLine("Sent: " + message);
    }

    private void ReceiveMessages()
    {
        try
        {
            while (true)
            {
                byte[] data = udpClient.Receive(ref localEndPoint);
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse(localEndPoint.Address.ToString()), localEndPoint.Port);
                string message = Encoding.UTF8.GetString(data);

                Console.WriteLine("Received: " + message);

                if (!connectedClients.Contains(clientEndPoint))
                {
                    connectedClients.Add(clientEndPoint);
                    Console.WriteLine("New client connected: " + clientEndPoint);
                }

                // Forward the message to all other clients
                foreach (IPEndPoint endpoint in connectedClients)
                {
                    if (!endpoint.Equals(clientEndPoint))
                    {
                        udpClient.Send(data, data.Length, endpoint);
                    }
                }
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine("SocketException: " + ex.Message);
        }
    }
}
}
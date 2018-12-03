using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;

namespace UDPProxy
{
    public class Proxy
    {
        private readonly int PORT;

        public Proxy(int port)
        {
            PORT = port;
        }

        private readonly string URL = "https://watermasterapi.azurewebsites.net/api/sensordata/";
        public void Start()
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any,0);
            using (UdpClient receiverSock = new UdpClient(PORT))
            {
                while (true)
                {
                    HandleOneRequest(receiverSock, remoteEP);
                }
            }
        }


        public void HandleOneRequest(UdpClient receiverSock, IPEndPoint remoteEP)
        {

            byte[] data = receiverSock.Receive(ref remoteEP);
            string inStr = Encoding.ASCII.GetString(data);

            Console.WriteLine("Modtaget" + inStr);
            Console.WriteLine("Sender IP: " + remoteEP.Address + "port: "+ remoteEP.Port);
            Console.WriteLine("");
            if (inStr !="")
            {
                Console.WriteLine(PostSensorData(inStr));
            }
            Console.WriteLine("");
        }

        public bool PostSensorData(string data)
        {
            StringContent content = new StringContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage resultMessage = client.PostAsync(URL, content).Result;
                    return resultMessage.IsSuccessStatusCode;
                }
                catch (Exception e)
                {
                    return false;
                    // Refactor to implement a system.diagnostics "Trace" class for logging errors
                }
            }
        }
            



        

        public bool PostSensorData()
        {
            return true;
        }
    }
}

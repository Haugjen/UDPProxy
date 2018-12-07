using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using SensorModels;

namespace UDPProxy
{
    public class Proxy
    {
        private readonly int PORT;

        public Proxy(int port)
        {
            PORT = port;
        }

        private readonly string URL = "https://watermasterapi.azurewebsites.net/api/";
        
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

            //Console.WriteLine("Modtaget: " + inStr);
            //Console.WriteLine("Sender IP: " + remoteEP.Address + " port: "+ remoteEP.Port);
            //Console.WriteLine("");
            if (inStr !="")
            {
                if (inStr.Length == 17)
                {
                    SetPiPort(inStr);
                }
                else
                {
                    Console.WriteLine("Has posted: " + PostSensorData(inStr));
                    SensorData sensorData = JsonConvert.DeserializeObject<SensorData>(inStr) ;
                    UpdateWaterPi(sensorData.FK_MacAddress);
                }
            }
            Console.WriteLine("");
        }
        public void UpdateWaterPi(string mac)
        {
            Console.WriteLine("Checking watering");
            string wateringJson = "";
            using (HttpClient client = new HttpClient())
            {
                wateringJson = client.GetStringAsync(URL + "weather/" + mac).Result;
            }

            Watering watering = JsonConvert.DeserializeObject<Watering>(wateringJson);
            Console.WriteLine("Should i water: " + watering.Water);

            byte[] data = Encoding.ASCII.GetBytes(watering.Water.ToString());
            IPEndPoint recieverEP = new IPEndPoint(IPAddress.Broadcast, watering.Port);

            using (UdpClient senderSock = new UdpClient())
            {
                senderSock.EnableBroadcast = true;
                senderSock.Send(data, data.Length, recieverEP);
            }
        }



        public bool PostSensorData(string data)
        {
            StringContent content = new StringContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage resultMessage = client.PostAsync(URL + "sensordata/", content).Result;
                    return resultMessage.IsSuccessStatusCode;
                }
                catch (Exception e)
                {
                    return false;
                    // Refactor to implement a system.diagnostics "Trace" class for logging errors
                }
            }
        }

        public void SetPiPort(string mac)
        {
            //PiPortModel ppm = null;
            string jsonPort = "";
            using (HttpClient client = new HttpClient())
            {
                jsonPort = client.GetStringAsync(URL + "sensor/port/" + mac).Result;
                //ppm = JsonConvert.DeserializeObject<PiPortModel>(jsonPort);
            }

            Console.WriteLine("Sender: " + jsonPort);
           
            byte[] data = Encoding.ASCII.GetBytes(jsonPort);
            IPEndPoint recieverEP = new IPEndPoint(IPAddress.Broadcast, 6000);

            using (UdpClient senderSock = new UdpClient())
            {
                senderSock.EnableBroadcast = true;
                senderSock.Send(data, data.Length, recieverEP);
                //IPEndPoint fromReciever = new IPEndPoint(IPAddress.Any, 0);
                //byte[] inData = senderSock.Receive(ref fromReciever);
            }
        }



    }
}

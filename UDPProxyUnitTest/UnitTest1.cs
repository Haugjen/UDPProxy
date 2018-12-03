using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SensorModels;
using UDPProxy;

namespace UDPProxyUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        Proxy proxy = new Proxy(6666);

        [TestMethod]
        public void SendAPITestMethod()
        {

            SensorData testData = new SensorData { Humidity = 2, Date = Convert.ToDateTime("02/04/2007 19.23.58"), FK_MacAddress = "00:20:18:61:f1:8a" };

            string testString = JsonConvert.SerializeObject(testData);

            Assert.AreEqual(true, proxy.PostSensorData(testString));
        }
    }
}

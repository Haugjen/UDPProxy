using System;

namespace UDPProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            Proxy proxy = new Proxy(6666);
            proxy.Start();
            Console.ReadKey();
        }
    }
}

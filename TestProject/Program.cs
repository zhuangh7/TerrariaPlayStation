using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000));
            byte[] result = new byte[1024];
            int length = clientSocket.Receive(result);
            string getString = Encoding.UTF8.GetString(result, 0, length);
            Console.WriteLine("Connect successfully and get: {0}", getString);
            clientSocket.Close();
            Console.WriteLine("press any key to exit");
            Console.Read();
        }
    }
}

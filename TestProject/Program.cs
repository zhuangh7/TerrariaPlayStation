using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestProject
{
    class Program
    {
        public static void writeRead(Object obj)
        {
            Socket client = (Socket)obj;
            while (true)
            {
                string s = Console.ReadLine();
                client.Send(System.Text.Encoding.ASCII.GetBytes(s));
                Console.WriteLine("sent one msg");
            }
        }
        public static void PlayTerrariaTogetherTestFunction(string[] args) {

            if (args.Length == 1) {
                //模拟host
                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777));  //绑定IP地址：端口
                serverSocket.Listen(10);//设定最多10个排队连接请求 
                Console.WriteLine("try as server");
                Socket client = serverSocket.Accept();
                Console.WriteLine("get connect");
                byte[] msg = new byte[1024];
                int length;
                Thread thread = new Thread(new ParameterizedThreadStart(writeRead));
                thread.Start(client);

                while (true) {
                    length = client.Receive(msg);
                    Console.WriteLine("msg receive:");
                    Console.Write(System.Text.Encoding.ASCII.GetString(msg));
                    msg = new byte[1024];
                }
            } else {
                //模拟client
                Console.WriteLine("try as client");
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9527));
                Console.WriteLine("success connect to 9527");
                byte[] msg = new byte[1024];
                int length;
                Thread thread = new Thread(new ParameterizedThreadStart(writeRead));
                thread.Start(clientSocket);
                Console.WriteLine("start pipe");
                while (true) {
                    length = clientSocket.Receive(msg);
                    Console.WriteLine("msg receive:");
                    Console.Write(System.Text.Encoding.ASCII.GetString(msg));
                    msg = new byte[1024];
                }

            }
        }
        public static void ZPMTestFunc(string[] args) {
                    }
        static void Main(string[] args)
        {
            //PlayTerrariaTogetherTestFunction(args);
            ZPMTestFunc(args);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCClient_v1
{
    class Program
    {
        public static bool running = true;

        public static void pipe(Object obj)
        {
            Pipe p = (Pipe)obj;
            byte[] msg = new byte[1];
            int length;
            while (running)
            {
                length = p.client.Receive(msg);
                p.host.Send(msg);
            }
        }
        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                //Strat with ID
                try
                {
                    int id = int.Parse(args[0]);
                    if (id >= 128 || id < 0)
                    {
                        Console.WriteLine("ID illegal");
                    }
                    else
                    {
                        //start local host
                        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1030));  //绑定IP地址：端口
                        serverSocket.Listen(1);//设定最多10个排队连接请求

                        //connect to server
                        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        clientSocket.Connect(new IPEndPoint(IPAddress.Parse("123.206.208.46"), 1030));
                        byte[] msg = new byte[1];
                        try
                        {
                            int temp = id + 128;
                            byte idtosend = (byte)temp;
                            msg[0] = idtosend;
                            clientSocket.Send(msg);

                            Socket game = serverSocket.Accept();
                            int length;
                            Thread guard = new Thread(new ParameterizedThreadStart(pipe));

                            Console.WriteLine("Game Begin.");
                            guard.Start(new Pipe(clientSocket, game));

                            while (running)
                            {
                                length = clientSocket.Receive(msg);
                                game.Send(msg);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Connect refuse, may the id error or the host is realdy closed or some other problems.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The arg is illegal!");
                }
            }
            else
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse("123.206.208.46"), 1030));
                byte[] msg = new byte[0];
                byte idtosend = 0;
                msg[0] = idtosend;
                clientSocket.Send(msg);


            }
        }
    }
}

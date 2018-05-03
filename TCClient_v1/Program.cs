using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCClient_v1 {
    class Program {
        public static int PORT = 7777;//change to 7777 while pro
        public static string SERVER_IP = "123.206.208.46";//123.206.208.46
        public static bool running = true;

        public static void pipe(Object obj) {
            try {

                Pipe p = (Pipe)obj;
                byte[] msg = new byte[1024];
                byte[] tosent;
                int length;
                while (running) {
                    length = p.client.Receive(msg);
                    tosent = new byte[length];
                    Buffer.BlockCopy(msg, 0, tosent, 0, length);
                    p.host.Send(tosent);
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.WriteLine("tcp close");
                running = false;
            }
        }
        static void Main(string[] args) {
            if (args.Length >= 1) {
                //Strat with ID
                try {
                    int id = int.Parse(args[0]);
                    if (id >= 128 || id < 0) {
                        Console.WriteLine("ID illegal");
                    } else {
                        //start local host
                        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT));  //绑定IP地址：端口
                        serverSocket.Listen(1);//设定最多10个排队连接请求

                        Console.WriteLine("Start to connect server...");

                        //connect to server
                        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        clientSocket.Connect(new IPEndPoint(IPAddress.Parse(SERVER_IP), 1030));
                        Console.WriteLine("Connect server successfully.");

                        byte[] msg = new byte[1];
                        try {
                            int temp = id + 128;//set first bit to 1
                            byte idtosend = (byte)temp;
                            msg[0] = idtosend;
                            clientSocket.Send(msg);

                            Console.WriteLine("Please start game, select join in IP, and type 127.0.0.1:{0}", PORT);
                            Socket game = serverSocket.Accept();
                            int length;
                            Thread guard = new Thread(new ParameterizedThreadStart(pipe));

                            Console.WriteLine("Game Begin.");
                            guard.Start(new Pipe(clientSocket, game));
                            msg = new byte[1024];
                            byte[] tosent;
                            while (running) {
                                //Console.WriteLine("main thread transle.");
                                length = clientSocket.Receive(msg);
                                tosent = new byte[length];
                                Buffer.BlockCopy(msg, 0, tosent, 0, length);
                                game.Send(tosent);
                            }
                        } catch (Exception e) {
                            Console.WriteLine("Connect refuse, may the id error or the host is realdy closed or some other problems.");
                        }
                    }
                } catch (Exception e) {
                    Console.WriteLine(e);
                    Console.WriteLine("The arg is illegal!");
                }
            } else {
                try {

                    Console.WriteLine("Start to connect server...");
                    Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    clientSocket.Connect(new IPEndPoint(IPAddress.Parse(SERVER_IP), 1030));

                    Console.WriteLine("Connect server successfully.");
                    byte[] msg = new byte[1];
                    byte idtosend = 0;
                    msg[0] = idtosend;
                    clientSocket.Send(msg);
                    //connect to server with cmd 0

                    int length = clientSocket.Receive(msg);
                    int id = msg[0];
                    //get room id from server

                    Console.WriteLine("Your room ID is: {0}", id);

                    Console.WriteLine("Please start game and select host game, press anykey when you successfully host the game");
                    Console.ReadKey();
                    Console.WriteLine("Coneecting to local game...");
                    Socket game = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    bool findgame = false;
                    while (!findgame) {
                        try {
                            game.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777));
                            Console.WriteLine("Local game finded.");
                            break;
                        } catch (Exception e) {
                            Console.WriteLine("Find local game fall.");
                        }
                    }
                    bool gamestart = false;
                    Thread guard = new Thread(new ParameterizedThreadStart(pipe));
                    guard.Start(new Pipe(clientSocket, game));
                    while (running) {
                        //Console.WriteLine("main thread transle.");
                        length = clientSocket.Receive(msg);
                        if (!gamestart) {
                            gamestart = true;
                            Console.WriteLine("Game start.");
                        }
                        game.Send(msg);
                    }
                } catch (Exception e) {
                    Console.WriteLine(e.ToString());
                    running = false;
                }
            }
            Console.WriteLine("press any key to exit");
            Console.Read();
        }
    }
}

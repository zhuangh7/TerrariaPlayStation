using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TerrariaConeecter {
    class Program {
        public static Nullable<TimeSpan>[] times = new TimeSpan?[128];
        public static Socket[] hosts = new Socket[128];
        public static Thread[] rooms = new Thread[128];
        public static Boolean running = true;
        public static int TIME_OUT = 10;//min

        public static void TimeWatcher() {
            TimeSpan now;
            TimeSpan diff;
            while (running) {
                now = new TimeSpan(DateTime.Now.Ticks);
                for (int i = 0; i < 128; i++) {
                    if (hosts[i] != null && rooms[i] == null && times[i] != null) {
                        //host come and waitting client...
                        diff = now.Subtract(times[i].Value);
                        if (diff.TotalMinutes > TIME_OUT) {
                            //this room timeout, clear forciblly
                            hosts[i] = null;
                            times[i] = null;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("room: {0} is time out, clear succefully.", i);
                        }
                    }
                    if (hosts[i] != null && !hosts[i].Connected) {
                        hosts[i] = null;
                        times[i] = null;
                        rooms[i] = null;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("room: {0}'s host is disconnect, clear...", i);
                    }
                }
            }
        }
        public static void Clean(int i, Socket s1, Socket s2) {
            if (s1.Connected) {
                s1.Close();
            }
            if (s2.Connected) {
                s2.Close();
            }
            if (hosts[i] != null && hosts[i].Connected) {
                hosts[i].Close();
            }
            rooms[i] = null;
            hosts[i] = null;
            times[i] = null;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Room {0} clean.", i);
        }
        public static void PipeRoom(Object obj) {
            //if thread.Name >0 means this is the main pipe thread, and should self delete while timeout (position is name-1)
            //else means this is the second pipe thread, ez exit is OK.
            Pipe pipe = (Pipe)obj;
            int name = int.Parse(Thread.CurrentThread.Name);
            byte[] line = new byte[1024];
            byte[] tosent;
            int length;
            try {

                if (name > 0) {
                    //start the second thread
                    Thread second = new Thread(new ParameterizedThreadStart(PipeRoom));
                    second.Name = (-name).ToString();
                    second.Start(obj);
                    while (running) {
                        length = pipe.client.Receive(line);
                        if (length > 0) {
                            tosent = new byte[length];
                            Buffer.BlockCopy(line, 0, tosent, 0, length);
                            pipe.host.Send(tosent);
                        }
                    }
                } else {
                    while (running) {
                        length = pipe.host.Receive(line);
                        if (length > 0) {
                            tosent = new byte[length];
                            Buffer.BlockCopy(line, 0, tosent, 0, length);
                            pipe.client.Send(line);
                        }
                    }
                }

            } catch (Exception e) {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(e.ToString());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Room {0} closed with exception ", name);
                if (name < 0) {
                    name = -name;
                }
                Clean(name, pipe.client, pipe.host);
            }
        }

        public static void JudgeClientType(Object obj) {
            try {
                dynamic c = (Client)obj;
                Socket client = c.socket;
                byte[] get = new byte[1];
                int length = client.Receive(get);
                if (length == 1) {
                    byte cmd = get[0];
                    if (cmd < 128) {
                        if (cmd == 0) {
                            //come with zero
                            //if the client come with 0 before all bytes,
                            Boolean find = false;
                            for (int i = 0; i < 128; i++) {
                                if (hosts[i] == null) {
                                    //get an new ID code return to the client 
                                    byte[] msgtosent = new byte[1];
                                    byte idtosent = (byte)i;
                                    msgtosent[0] = idtosent;
                                    client.Send(msgtosent);
                                    //and put the client into rooms
                                    times[i] = new TimeSpan(DateTime.Now.Ticks);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    c.print(0);
                                    Console.WriteLine("Judge is Host client, give ROOM {0}", i);
                                    Console.WriteLine("Set timetag: {0} to socket Room {1}", times[i].ToString(), 1);
                                    hosts[i] = client;
                                    find = true;
                                    break;
                                }
                            }
                            if (!find) {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Rooms is full, refuse the connection.");
                                client.Close();
                            }
                        } else {
                            //client come with ID and begin with 0, this is a bak connect.
                            //* update in v2
                        }
                    } else {
                        //else if the client come with 1 before all bytes, search the ID Code in the 1-4 byte position
                        int id = cmd - 128;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Judge is Client client, give ROOM {0}", id);
                        if (hosts[id] == null) {
                            Console.ForegroundColor = ConsoleColor.Red;
                            c.print(-1);
                            Console.WriteLine("The room id <{0}> is wrong or the room is already close, refuse the connection.", id);
                            client.Close();
                        } else {
                            c.print(1);
                            if (rooms[id] == null) {
                                //  if get the ID and its Host Room, put into new PipeThread
                                Thread thread = new Thread(new ParameterizedThreadStart(PipeRoom));
                                rooms[id] = thread;
                                thread.Name = (id + 1).ToString();
                                thread.Start(new Pipe(hosts[id], client));
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Room start succefully.");
                            } else {
                                //  already one client connect to the host in room[id]
                                //*update in v2 edition
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Already one client in this room.");
                                client.Close();
                            }

                        }
                    }
                } else {
                    client.Close();
                }
                //  else close the connection
            } catch (Exception e) {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(e.ToString());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("不知道发生了什么，有点凉");
            }
        }
        public static ZProgramMoniter.Moniter fun = (args) => {
            if ((int)args.Arguments[0] != -1) {
                return true;
            } else {
                return false;
            }
        };
        static void Main(string[] args) {
            ZProgramMoniter.ZPM.RegisterMoniter("user_create", fun);

            Thread timewatcher = new Thread(new ThreadStart(TimeWatcher));
            timewatcher.Start();
            //start time watcher(gc)
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 1030));  //绑定IP地址：端口
            serverSocket.Listen(10);//设定最多10个排队连接请求 
            while (running) {
                try {
                    //start server and accept
                    Console.WriteLine("start listen in 1030");
                    Socket clientSocket = serverSocket.Accept();
                    //get new client
                    IPEndPoint clientAddr = (IPEndPoint)clientSocket.RemoteEndPoint;
                    string ipaddr = clientAddr.Address.ToString();
                    int port = clientAddr.Port;//get IP and port 
                    dynamic c = new Client();
                    c.IP = ipaddr;
                    c.PORT = port;
                    c.socket = clientSocket;
                    Thread thread = new Thread(new ParameterizedThreadStart(JudgeClientType));
                    thread.Start(c);
                } catch (Exception e) {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(e.ToString());
                }
            }
            serverSocket.Close();
            Console.WriteLine("press any key to exit");
            Console.Read();

        }
    }
}

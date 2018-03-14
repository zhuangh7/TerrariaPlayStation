using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TerrariaConeecter
{
    class Program
    {
        public static Nullable<TimeSpan>[] times = new TimeSpan?[128];
        public static Socket[] hosts = new Socket[128];
        public static Thread[] rooms = new Thread[128];
        public static Boolean running = true;
        public static int TIME_OUT = 10;//min

        public static void TimeWatcher()
        {
            TimeSpan now;
            TimeSpan diff;
            while (running)
            {
                now = new TimeSpan(DateTime.Now.Ticks);
                for (int i = 0; i < 128; i++)
                {
                    if (hosts[i] != null && rooms[i] == null && times[i] != null)
                    {
                        //host come and waitting client...
                        diff = now.Subtract(times[i].Value);
                        if (diff.TotalMinutes > TIME_OUT)
                        {
                            //this room timeout, clear forciblly
                            hosts[i] = null;
                            times[i] = null;
                            Console.WriteLine("room: {0} is time out, clear succefully.", i);
                        }
                    }
                    if (hosts[i] != null && !hosts[i].Connected)
                    {
                        hosts[i] = null;
                        times[i] = null;
                        rooms[i] = null;
                        Console.WriteLine("room: {0}'s host is disconnect, clear...", i);
                    }
                }
            }
        }
        public static void Clean(int i)
        {
            if (hosts[i] != null && hosts[i].Connected)
            {
                hosts[i].Close();
            }
            rooms[i] = null;
            hosts[i] = null;
            times[i] = null;
        }
        public static void PipeRoom(Object obj)
        {
            //if thread.Name >0 means this is the main pipe thread, and should self delete while timeout (position is name-1)
            //else means this is the second pipe thread, ez exit is OK.
            Pipe pipe = (Pipe)obj;
            try
            {
                int name = int.Parse(Thread.CurrentThread.Name);
                try
                {

                    if (name > 0)
                    {
                        //start the second thread
                        Thread second = new Thread(new ParameterizedThreadStart(PipeRoom));
                        second.Name = (-name).ToString();
                        second.Start(obj);

                        byte[] line = new byte[1];
                        int length;
                        while (running)
                        {
                            length = pipe.client.Receive(line);
                            if (length == 1)
                            {
                                pipe.host.Send(line);
                            }
                            if (!pipe.client.Connected || !pipe.host.Connected)
                            {
                                Console.WriteLine("close room: {0}", name);
                                //one is close, clear this room and host
                                if (name < 0)
                                {
                                    if (pipe.host.Connected)
                                    {
                                        pipe.host.Close();
                                    }
                                    if (pipe.client.Connected)
                                    {
                                        pipe.host.Close();
                                    }
                                    Clean(-name);
                                }
                                else
                                {
                                    if (pipe.host.Connected)
                                    {
                                        pipe.host.Close();
                                    }
                                    if (pipe.client.Connected)
                                    {
                                        pipe.host.Close();
                                    }
                                    Clean(name);
                                }
                            }
                        }
                    }
                    else
                    {
                        byte[] line = new byte[1];
                        int length;
                        while (running)
                        {
                            length = pipe.host.Receive(line);
                            if (length == 1)
                            {
                                pipe.client.Send(line);
                            }
                            if (!pipe.client.Connected || !pipe.host.Connected)
                            {
                                //one is close, clear this room and host
                                Console.WriteLine("close room: {0}", name);
                                if (name < 0)
                                {
                                    if (pipe.host.Connected)
                                    {
                                        pipe.host.Close();
                                    }
                                    if (pipe.client.Connected)
                                    {
                                        pipe.host.Close();
                                    }
                                    Clean(-name);
                                }
                                else
                                {
                                    if (pipe.host.Connected)
                                    {
                                        pipe.host.Close();
                                    }
                                    if (pipe.client.Connected)
                                    {
                                        pipe.host.Close();
                                    }
                                    Clean(name);
                                }
                            }

                        }
                    }

                }
                catch (Exception e)
                {
                    Console.Write(e.ToString());
                    Console.WriteLine("one pipe line is closing.");
                    if (name < 0)
                    {
                        name = -name;
                    }
                    Clean(name);
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                Console.WriteLine("create pipeline error, close two socket.");
                pipe.host.Close();
                pipe.client.Close();
            }
        }

        public static void JudgeClientType(Object obj)
        {
            try
            {
                Socket client = (Socket)obj;
                byte[] get = new byte[1];
                int length = client.Receive(get);
                if (length == 1)
                {
                    byte cmd = get[0];
                    if (cmd < 128)
                    {
                        if (cmd == 0)
                        {
                            //come with zero
                            //if the client come with 0 before all bytes,
                            Boolean find = false;
                            for (int i = 0; i < 128; i++)
                            {
                                if (hosts[i] == null)
                                {
                                    //get an new ID code return to the client 
                                    byte[] msgtosent = new byte[1];
                                    byte idtosent = (byte)i;
                                    msgtosent[0] = idtosent;
                                    client.Send(msgtosent);
                                    //and put the client into rooms
                                    times[i] = new TimeSpan(DateTime.Now.Ticks);
                                    Console.WriteLine("Set timetag: {0}", times[i].ToString());
                                    hosts[i] = client;
                                    find = true;
                                    break;
                                }
                            }
                            if (!find)
                            {
                                Console.WriteLine("The rooms is full, refuse the connection.");
                                client.Close();
                            }
                        }
                        else
                        {
                            //client come with ID and begin with 0, this is a bak connect.
                            //* update in v2
                        }
                    }
                    else
                    {
                        //else if the client come with 1 before all bytes, search the ID Code in the 1-4 byte position
                        int id = cmd - 128;
                        if (hosts[id] == null)
                        {
                            Console.WriteLine("The room id is wrong or the room is already close, refuse the connection.");
                            client.Close();
                        }
                        else
                        {
                            if (rooms[id] == null)
                            {
                                //  if get the ID and its Host Room, put into new PipeThread
                                Thread thread = new Thread(new ParameterizedThreadStart(PipeRoom));
                                rooms[id] = thread;
                                thread.Name = (id + 1).ToString();
                                thread.Start(new Pipe(hosts[id], client));
                            }
                            else
                            {
                                //  already one client connect to the host in room[id]
                                //*update in v2 edition
                                client.Close();
                            }

                        }
                    }
                }
                else
                {
                    client.Close();
                }
                //  else close the connection
            }catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("不知道发生了什么，有点凉");
            }
        }
        static void Main(string[] args)
        {
            Thread timewatcher = new Thread(new ThreadStart(TimeWatcher));
            timewatcher.Start();
            //start time watcher(gc)

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 1030));  //绑定IP地址：端口
            serverSocket.Listen(10);//设定最多10个排队连接请求 
            while (running)
            {
                try
                {

                    Console.WriteLine("start listen...");
                    //start server and accept
                    Socket clientSocket = serverSocket.Accept();
                    //get new client
                    IPEndPoint clientAddr = (IPEndPoint)clientSocket.RemoteEndPoint;
                    string ipaddr = clientAddr.Address.ToString();
                    int port = clientAddr.Port;//get IP and port 
                    Console.WriteLine("new client come with ip: {0} and port: {1}", ipaddr, port);
                    Thread thread = new Thread(new ParameterizedThreadStart(JudgeClientType));
                    thread.Start(clientSocket);
                }
                catch (Exception e)
                {
                    Console.Write(e.ToString());
                }
            }
            serverSocket.Close();
            Console.WriteLine("press any key to exit");
            Console.Read();
        }
    }
}

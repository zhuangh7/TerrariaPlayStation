using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZProgramMoniter;

namespace TestProject {
    class Program {
        public static void writeRead(Object obj) {
            Socket client = (Socket)obj;
            while (true) {
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
            ZPM.Print();
        }
        public static void AOP_1TestFunc(string[] args) {
            dynamic aop = new AOP_1();
            aop.FUN();
        }
        public static void postFunc() {
            var factory = new ConnectionFactory() { HostName = "www.zhuangh7.cn", UserName = "test", Password = "test" };
            using (var connection = factory.CreateConnection()) {
                using (var channel = connection.CreateModel()) {
                    channel.QueueDeclare(queue: "test",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message = "{\"msg\":\"send from ZPM for JSON test\"}";
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "test",
                                         basicProperties: null,
                                         body: body);
                    Console.WriteLine(" [x] Sent {0}", message);
                }
            }
        }
        public static void connectToRemoteRabbitMQTestFunc(string[] args) {
            Thread t1 = new Thread(new ThreadStart(postFunc));
            t1.Start();
        }
        public static void MC() {
            InputNode I1 = new InputNode();
            InputNode I2 = new InputNode();
            OutputNode O = new OutputNode(2);
            O.addInputNode(I1);
            O.addInputNode(I2);
            //create mc


            while (true) {

                if (train(I1, I2, O, 0, 0, 0))
                    break;
                if (train(I1, I2, O, 0, 1, 0))
                    break;
                if (train(I1, I2, O, 1, 0, 0))
                    break;
                if (train(I1, I2, O, 1, 1, 1))
                    break;
            }
        }
        public static bool train(InputNode I1, InputNode I2, OutputNode O, double input1, double input2, double output) {

            double alpha = 0.015;
            double beta = 0.006;
            double r;
            double e;
            I1.changeData(input1);
            I2.changeData(input2);
            r = O.getResult();
            e = output - r;

            I1.changeWeight(I1.weight + alpha * input1 * e);
            I2.changeWeight(I2.weight + alpha * input2 * e);
            O.Theat = O.Theat + beta * e;
            Console.WriteLine($"{I1.weight} , {I2.weight} , {r} , {e}");
            if (e > 0 && e < 0.000001) {
                return true;
            } else if (e < 0 && e > -0.000001) {
                return true;
            } else {
                return false;
            }
        }
        static void Main(string[] args) {
            PlayTerrariaTogetherTestFunction(args);
            //ZPMTestFunc(args);
            //AOP_1TestFunc(args);
            //connectToRemoteRabbitMQTestFunc(args);
            //MC();
            Console.WriteLine("type to exit");
            Console.ReadKey();
        }
    }
}

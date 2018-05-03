using System;
using System.Collections.Generic;
using System.Text;
using KingAOP.Aspects;
using RabbitMQ.Client;

namespace ZProgramMoniter {
    public delegate bool Moniter(MethodExecutionArgs args);
    public class ZPM {
        private static Dictionary<string,Moniter> moniters = new Dictionary<string,Moniter>();
        public static void Print() {
            Console.WriteLine("testLibrary2");
        }
        public static void RegisterMoniter(string name,Moniter m) {
            moniters.Add(name, m);
        }
        public static Moniter getMoniter(string name) {
            if (moniters.ContainsKey(name)) {
                return moniters[name];
            } else {
                return null;
            }
        }
        public static void sendMessageToNormal(object fun) {
            var factory = new ConnectionFactory() { HostName = "www.zhuangh7.cn", UserName = "test", Password = "test" };
            using (var connection = factory.CreateConnection()) {
                using (var channel = connection.CreateModel()) {
                    channel.QueueDeclare(queue: "test",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message = $"{{\"type\":\"ZPM\",\"call_function\":\"{fun}\"}}";
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "test",
                                         basicProperties: null,
                                         body: body);
                    Console.WriteLine(" [x] Sent {0}", message);
                }
            }
        }
        public static void sendMessageToWarnning(object fun) {
            var factory = new ConnectionFactory() { HostName = "www.zhuangh7.cn", UserName = "test", Password = "test" };
            using (var connection = factory.CreateConnection()) {
                using (var channel = connection.CreateModel()) {
                    channel.QueueDeclare(queue: "warnning",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message = $"{{\"type\":\"ZPM_W\",\"call_function\":\"{fun}\"}}";
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "warnning",
                                         basicProperties: null,
                                         body: body);
                    Console.WriteLine(" [x] Sent Warnning {0}", message);
                }
            }
        }
    }
}

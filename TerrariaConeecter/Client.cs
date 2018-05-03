using KingAOP;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TerrariaConeecter {
    class Client : IDynamicMetaObjectProvider {
        public string IP;
        public int PORT;
        public int COUNT;
        public Socket socket;
        public Client() {
        }

        [ZProgramMoniter.ZPMAspect("user_create")]
        public void print(int i) {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("new client come with ip: {0} and port: {1}.({2})", IP, PORT, COUNT);
        }
        public DynamicMetaObject GetMetaObject(Expression parameter) {
            return new AspectWeaver(parameter, this);
        }
    }
}

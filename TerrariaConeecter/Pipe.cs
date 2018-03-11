using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TerrariaConeecter
{
    class Pipe
    {
        public Socket host { set; get; }
        public Socket client { set; get; }
        public Pipe(Socket h,Socket c)
        {
            this.host = h;
            this.client = c;
        }
    }
}

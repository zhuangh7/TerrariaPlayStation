using KingAOP.Aspects;
using System;
using System.Threading;

namespace ZProgramMoniter {
    public class ZPMAspect : OnMethodBoundaryAspect {
        string name;
        public ZPMAspect(string fun) {
            this.name = fun;
        }
        public override void OnEntry(MethodExecutionArgs args) {
            SendNormalLog(this.FunctionName);
            Moniter moniter = ZProgramMoniter.ZPM.getMoniter(name);
            if (!moniter(args)) {
                SendWarnningLog(this.FunctionName);
            }
        }

        public override void OnSuccess(MethodExecutionArgs args) {
        }

        public override void OnExit(MethodExecutionArgs args) {
        }

        private void SendNormalLog(string fun) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("ZPM LOG SENT");
            try {
                Thread t1 = new Thread(new ParameterizedThreadStart(ZPM.sendMessageToNormal));
                t1.Start(fun);
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }
        private void SendWarnningLog(string fun) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("ZPM WARNNING SENT");
            try {
                Thread t1 = new Thread(new ParameterizedThreadStart(ZPM.sendMessageToWarnning));
                t1.Start(fun);
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}

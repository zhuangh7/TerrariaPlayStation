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
            try {
                ZPM.SendNormalLog($"\"function_name\":\"{this.FunctionName}\",\"client_ip\":\"{args.Arguments[1]}\",\"client_port\":\"{args.Arguments[2]}\"");
                Moniter moniter = ZProgramMoniter.ZPM.getMoniter(name);
                if (!moniter(args)) {
                    ZPM.SendWarnningLog($"\"function_name\":\"{this.FunctionName}\",\"client_ip\":\"{args.Arguments[1]}\",\"client_port\":\"{args.Arguments[2]}\"");
                }
            } catch (Exception e) {
                Console.WriteLine("error happened");
            }
        }

        public override void OnSuccess(MethodExecutionArgs args) {
        }

        public override void OnExit(MethodExecutionArgs args) {
        }


    }
}

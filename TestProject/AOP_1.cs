using KingAOP;
using System;
using System.Dynamic;
using System.Linq.Expressions;
using ZProgramMoniter;

namespace TestProject {
    class AOP_1 : IDynamicMetaObjectProvider {
        public Moniter fun = (args) => {
            Console.WriteLine("this is moniter");
            return false;
        };

        public AOP_1() {
            ZProgramMoniter.ZPM.RegisterMoniter("moniter_1", fun);
        }

        [ZPMAspect("moniter_1")]
        public void FUN() {
            Console.WriteLine("this is FUN from AOP_1");
        }

        public DynamicMetaObject GetMetaObject(Expression parameter) {
           return new AspectWeaver(parameter, this);
        }
    }
}

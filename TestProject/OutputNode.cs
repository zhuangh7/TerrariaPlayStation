using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject {
    class OutputNode {
        public InputNode[] inputs;
        public double Theat;
        private int numInput = 0;
        public OutputNode(int num) {
            Random r = new Random();
            Theat = r.NextDouble()*2-1;
            inputs = new InputNode[num];
        }
        public bool addInputNode(InputNode input) {
            if (numInput < inputs.Length) {
                inputs[numInput++] = input;
                return true;
            }
            return false;
        }
        public double getResult() {
            double result = 0;
            if(numInput != inputs.Length) {
                throw new Exception("input Nodes init needed");
            } else {
                foreach(InputNode i in inputs) {
                    result += i.active();
                }
                result -= Theat;
                if (result > 0) {
                    return 1;
                } else {
                    return 0;
                }
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject {
    class InputNode {
        public double weight {  get; private set; }
        public double data { get; set; }
        public InputNode() {
            Random random = new Random();
            weight = random.NextDouble()*2-1;
            //init node data
        }
        public void changeWeight(double newWeight) {
            this.weight = newWeight;
        }
        public void changeData(double newData) {
            this.data = newData;
        }
        public double active() {
            return data * weight;
        }
    }
}

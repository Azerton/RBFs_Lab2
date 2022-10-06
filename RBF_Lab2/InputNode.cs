using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBF_Lab2
{
    public class InputNode : NeuralNode
    {
        private List<double> outputs;
        public InputNode(List<double> outputs)
        {
            this.outputs = outputs;
        }

        public double NodeOutput(int testNum)
        {
            return (double)outputs[testNum];
        }
    }
}

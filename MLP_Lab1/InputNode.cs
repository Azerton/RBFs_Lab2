using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLP_Lab1
{
    public class InputNode : NeuralNode
    {
        private List<int> outputs;
        public InputNode(List<int> outputs)
        {
            this.outputs = outputs;
        }

        public void AssignOutputs(List<int> outputs)
        {
            this.outputs = outputs;
        }

        public double NodeOutput(int testNum)
        {
            return (float)outputs[testNum];
        }
    }
}

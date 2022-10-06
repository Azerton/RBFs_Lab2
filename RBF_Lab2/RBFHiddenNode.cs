using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RBF_Lab2
{
    public class RBFHiddenNode : NeuralNode
    {
        private NeuralNode input;
        private double gauWidth, center;

        public RBFHiddenNode(NeuralNode input)
        {
            this.input = input;
        }

        public NeuralNode GetInput()
        {
            return input;
        }

        public double NodeOutput(int testNum)
        {
            double power;
            double sqrNorm = input.NodeOutput(testNum) - center;
            power = (-1 / (2 * gauWidth * gauWidth)) * sqrNorm;
            return Math.Exp(power);
        }
    }
}

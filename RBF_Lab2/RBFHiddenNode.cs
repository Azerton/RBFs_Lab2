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

        public RBFHiddenNode(NeuralNode input, (double, double) centerAndWidth)
        {
            this.input = input;
            this.gauWidth = centerAndWidth.Item2;
            this.center = centerAndWidth.Item1;
        }

        public double GetGauWidth()
        {
            return gauWidth;
        }

        public double GetCenter()
        {
            return center;
        }

        public NeuralNode GetInput()
        {
            return input;
        }

        public double NodeOutput(int testNum)
        {
            double power;
            double sqrNorm = (input.NodeOutput(testNum) - center);
            power = (-1f / (2f * gauWidth * gauWidth)) * sqrNorm * sqrNorm;
            return Math.Exp(power);
        }
    }
}

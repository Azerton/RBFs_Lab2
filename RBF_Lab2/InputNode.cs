using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBF_Lab2
{
    public class InputNode : NeuralNode
    {
        private List<(double, double)> originalPointsList;
        public InputNode(List<(double, double)> originalPointsList)
        {
            this.originalPointsList = originalPointsList;
        }

        public double NodeOutput(int testNum)
        {
            return originalPointsList[testNum].Item1;
        }
    }
}

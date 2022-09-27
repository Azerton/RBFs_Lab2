using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLP_Lab1
{
    public class LearnableNode : NeuralNode
    {
        private Queue<(NeuralNode, float)> inputs;
        private float bias;
        public LearnableNode(Queue<(NeuralNode, float)> inputs)
        {
            this.inputs = inputs;
        }

        public void AssignInputs(Queue<(NeuralNode, float)> inputs)
        {
            this.inputs = inputs;
        }

        public float NodeOutput(int testNum)
        {
            //TODO: Add math for generating the output of the node
            return 0;
        }

        public void BackTrackLearn()
        {
            //TODO: Add math for learning of the neural network
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RBF_Lab2
{
    public class LearnableNode : NeuralNode
    {
        private Queue<(NeuralNode, double)> inputs;
        private Queue<double> learningVals;
        private double bias;
        public LearnableNode(Queue<(NeuralNode, double)> inputs, double bias)
        {
            this.inputs = inputs;
            this.bias = bias;
            learningVals = new Queue<double>();
        }

        public double GetBias()
        {
            return bias;
        }

        public Queue<(NeuralNode, double)> GetInputs()
        {
            return inputs;
        }

        public double NodeOutput(int testNum)
        {
            //Obtain weighted sum of all inputs
            double inputSum = 0;
            foreach ((NeuralNode, double) node in inputs)
            {
                inputSum += node.Item1.NodeOutput(testNum) * node.Item2;
            }

            //Apply the bias to the weighted sum
            inputSum += bias;

            //Return the logitic sigmoid function running off of the weighted sum
            return inputSum;
        }

        public void OutToLearnFrom(double val)
        {
            learningVals.Enqueue(val);
        }

        public void BackTrackLearn(int testNum, double learningRate)
        {
            //changeW for W of input x = lr * current node delta * input node x's output value
            //current node delta = ...
            //For output layer --> ... = current node output * (1 - current node output) * (expected output for testNum - current node output)
            //For hidden layer --> ... = current node output * (1 - current node output) * [sum of (weight going to node k * node k delta) for all k]

            double nodeOutput = this.NodeOutput(testNum);

            //Obtain [sum of (weight going to node k * node k delta) for all k]
            double sum = 0;
            foreach (double val in learningVals)
            {
                sum += val;
            }

            //Get current node delta using [sum of (weight going to node k * node k delta) for all k]
            double nodeD = nodeOutput * (1.0 - nodeOutput) * sum;

            //Apply learning to weights
            int inputCnt = inputs.Count;
            for (int i = 0; i < inputCnt; i++)
            {
                (NeuralNode, double) node = inputs.Dequeue();
                //If the input node is a learning node, then it will have to have the (weight between this and node.Item1 * this delta)
                if (node.Item1 is LearnableNode)
                {
                    //Give the current node's weighted delta (weight is unchanged still) to next node back for it to learn from when doing its weighted sum
                    ((LearnableNode)node.Item1).OutToLearnFrom(node.Item2 * nodeD);
                }
                //current node's weight for input node = current weight + learning rate * current node delta * input node's output
                node.Item2 = node.Item2 + (learningRate * nodeD * node.Item1.NodeOutput(testNum));
                inputs.Enqueue(node);
            }

            //Apply learning to bias
            bias = bias + (learningRate * nodeD);
            learningVals.Clear();
        }

        public void RBFLearn(int testNum, double learningRate, double error)
        {
            //Apply learning to weights
            int inputCnt = inputs.Count;
            for (int i = 0; i < inputCnt; i++)
            {
                (NeuralNode, double) node = inputs.Dequeue();

                //current node's weight for input node = current weight + learning rate * current node delta * input node's output
                node.Item2 = node.Item2 + (learningRate * error * node.Item1.NodeOutput(testNum));
                inputs.Enqueue(node);
            }

            //Apply learning to bias
            bias = bias + (learningRate * error);
            learningVals.Clear();
        }
    }
}


/*
 * deltak = ek(n) * phik(n)
 * ek(n) = dk - yk
 * phik(n) = a * yk * (1 - yk)
 */
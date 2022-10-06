using System;
using System.IO;
using System.Text;
using static System.Net.WebRequestMethods;

namespace RBF_Lab2
{
    public class NetworkController
    {
        int numEpochs, testsPerEpoch, numGauCenters;
        double learnRate;
        Stack<Queue<(NeuralNode, double)>> allNodesAndInputs;
        Queue<(NeuralNode, double)> lastInputLayer, currWorkingLayer;
        NeuralNode outputNode;
        List<(double, double)> generatedPoints;
        Random rand;    //Used for selecting random weight values

        public NetworkController()
        {
            allNodesAndInputs = new Stack<Queue<(NeuralNode, double)>>();
            lastInputLayer = new Queue<(NeuralNode, double)>();
            currWorkingLayer = new Queue<(NeuralNode, double)>();
            generatedPoints = new List<(double, double)>();
            rand = new Random();
        }

        //Will obtain the info to fill in numInputs, numLayers, nodesPerLayer, lrChange, weightMomentum, lrBounds, and inputValuesLists
        public void ObtainBaseInfo()
        {
            int inputTries = 0;
            string userInput = "";

            //Obtains number of nodes in hidden layer
            inputTries = 0;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain an integer from input, please try again.");
                Console.Write("Enter number of nodes (Gaussian Centers) for hidden layer [Integer]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!int.TryParse(userInput, out numGauCenters));

            //Obtains learning rate
            inputTries = 0;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain a float from input, please try again.");
                Console.Write("Enter the learning rate [Double]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!double.TryParse(userInput, out learnRate));

            //Obtains number of tests per epoch
            inputTries = 0;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain an integer from input, please try again.");
                Console.Write("Enter number of tests (Number of random points from original function) per epoch [Integer]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!int.TryParse(userInput, out testsPerEpoch));

            //Obtains number of epochs to train for
            inputTries = 0;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain an integer from input, please try again.");
                Console.Write("Enter the number of epochs to train for [Integer]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!int.TryParse(userInput, out numEpochs));
        }

        //Generates the Neural Network
        public void GenerateNetwork()
        {
            //Generate the list of random points
            GeneratePoints();

            //The base input node is created
            lastInputLayer.Enqueue((new InputNode(generatedPoints), 1.0));
            allNodesAndInputs.Push(lastInputLayer);

            //TODO: K-means algorithm to find the GausCenters for the RBFNodes

            //Now create the all the hidden nodes
            currWorkingLayer = new Queue<(NeuralNode, double)>();
            for (int j = 0; j < numGauCenters; j++)
            {
                currWorkingLayer.Enqueue((new RBFHiddenNode(lastInputLayer.Peek().Item1), RndWeightBias()));
            }

            //Move on to the output layer
            allNodesAndInputs.Push(currWorkingLayer);
            lastInputLayer = currWorkingLayer;

            //Create ouput layer node
            currWorkingLayer = new Queue<(NeuralNode, double)>();
            currWorkingLayer.Enqueue((new LearnableNode(lastInputLayer, RndWeightBias()), RndWeightBias()));

            //Select output node to start learning operation on
            outputNode = currWorkingLayer.Peek().Item1;
            PresentNodeNetwork();
        }

        //Generates the random points from the original function
        private void GeneratePoints()
        {
            for (int i = 0; i < testsPerEpoch; i++)
            {
                double x, y, isPos, yNoise;
                x = rand.NextDouble();  //Choose x value between 0.0 and 1.0
                y = 0.5 + (0.4 * Math.Sin(2 * Math.PI * x));    //Find y value for the x value
                yNoise = rand.NextSingle() / 10.0;  //Get amount of noise, -0.1 to 0.1, for the y value 
                isPos = rand.Next(2);  //Chooses either 1 or 0
                if (isPos == 0) yNoise = yNoise * -1;   //If isPos was chosen to be 0, then the noise is negative
                y += yNoise;    //Add the noise to the y value
                generatedPoints.Add((x, y));    //Add point with noise into the system
            }
        }

        //Runs the training of the RBF network
        public void RunTraining()
        {
            int numEpochsRun = 0;
            List<(double, double)> testResults = new List<(double, double)>();
            Console.WriteLine("Running training");
            List<int> testingOrder = new List<int>();
            while (numEpochsRun < numEpochs)
            {
                testingOrder = new List<int>();
                for (int i = 0; i < testsPerEpoch; i++)
                {
                    testingOrder.Add(i);
                }
                testingOrder = Shuffle(testingOrder);
                numEpochsRun++;
                testResults = RunEpoch(testingOrder);
            }

            //Exit training as it has finished successfully
            Console.WriteLine("Training has finished");
            Console.WriteLine("Learning Rate: " + learnRate);
            Console.WriteLine("///////////////////////////////////////////////////////////////////////////////////");
            Console.WriteLine("Sample points in test order for Epoch " + numEpochsRun + ": ");
            int testNum = 1;
            foreach (int val in testingOrder)
            {
                Console.WriteLine("\tTest Point " + testNum + ": " + generatedPoints[val]);
                testNum++;
            }
            Console.WriteLine("///////////////////////////////////////////////////////////////////////////////////");
            Console.WriteLine("Produced points in test order for Epoch " + numEpochsRun + ": ");
            for (int i = 1; i <= testsPerEpoch; i++)
            {
                Console.WriteLine("\tProduced Point " + i + ": " + testResults[i]);
            }
            Console.WriteLine("-----------------------------------------------------------------------------------");
        }

        //Runs an epoch, returns the list of points generated for the epoch
        public List<(double, double)> RunEpoch(List<int> testingOrder)
        {
            List<(double, double)> testPoints = new List<(double, double)>();

            foreach (int i in testingOrder)
            {
                //Get the output for the test
                double output = outputNode.NodeOutput(i);

                //Add the produced point to the list of outputs for the epoch
                testPoints.Add((generatedPoints[i].Item1, output));

                //Train the initial output node
                double outputd = generatedPoints[i].Item2 - output;   //The error between the expected output and the actual output
                ((LearnableNode)outputNode).OutToLearnFrom(outputd);    //Add the error of the output to the queue of error values that the node will need to learn from
                ((LearnableNode)outputNode).RBFLearn(i, learnRate);     //Perform learning on the output node
            }

            return testPoints;  //Return the produced points for the epoch
        }

        //Used to shuffle lists, in particular the list of order of test inputs
        public List<int> Shuffle(List<int> list)
        {
            List<int> result = new List<int>(list);
            List<int> temp = new List<int>(list);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                int value = temp[k];
                temp[k] = temp[n];
                temp[n] = value;
            }
            result = temp;
            return result;
        }

        //Used to select a random double between -1.0 and 1.0 to be used as an initial weight or bias
        private double RndWeightBias()
        {
            double weight = 0;
            weight = rand.NextDouble();
            int isPos = rand.Next(2);  //Chooses either 1 or 0
            if (isPos == 0) weight = weight * -1;
            return weight;
        }
        
        //Can be used to output the generated Node Network
        private void PresentNodeNetwork()
        {
            Console.WriteLine("Output node bias = " + ((LearnableNode)outputNode).GetBias());
            int learnNodeNum = 1;
            foreach ((NeuralNode, double) node in ((LearnableNode)outputNode).GetInputs())
            {

                Console.WriteLine("Hidden node " + learnNodeNum + " bias = " + ((LearnableNode)node.Item1).GetBias() + " and weight = " + node.Item2);
                int inputNodeNum = 1;
                foreach ((NeuralNode, double) inputNode in ((LearnableNode)node.Item1).GetInputs())
                {
                    Console.WriteLine("Input node " + inputNodeNum + " weight = " + inputNode.Item2);
                    inputNodeNum++;
                }
                learnNodeNum++;
            }
        }
    }
}

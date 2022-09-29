// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Text;
using static System.Net.WebRequestMethods;

namespace MLP_Lab1
{
    public class NetworkController
    {
        int numInputs, numLayers, epochs, testsPerEpoch;
        int[] nodesPerLayer;    //NOTE: Code can still run if last layer is more than 1 node, but will only take output from one output layer node (Specifically the first one created during Network Generation)
        double weightMomentum, lrCurr, absErr;
        Stack<Queue<(NeuralNode, double)>> allNodesAndInputs;
        Queue<(NeuralNode, double)> lastInputLayer, currWorkingLayer;
        NeuralNode outputNode;
        List<List<double>> inputValuesLists, inputValuesForTests;
        List<double> testAnswers;
        Random rand;    //Used for selecting random weight values

        public NetworkController()
        {
            inputValuesLists = new List<List<double>>();
            inputValuesForTests = new List<List<double>>();
            testAnswers = new List<double>();
            allNodesAndInputs = new Stack<Queue<(NeuralNode, double)>>();
            lastInputLayer = new Queue<(NeuralNode, double)>();
            currWorkingLayer = new Queue<(NeuralNode, double)>();
            rand = new Random();
        }

        //Will obtain the info to fill in numInputs, numLayers, nodesPerLayer, lrChange, weightMomentum, lrBounds, and inputValuesLists
        public int ObtainBaseInfo()
        {
            int inputTries = 0;
            string userInput = "";
            //Obtains number of input nodes
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain an integer from input, please try again.");
                Console.Write("Enter number of input nodes [Integer]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!int.TryParse(userInput, out numInputs));

            //Obtains number of node layers, does not count the input nodes
            inputTries = 0;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain an integer from input, please try again.");
                Console.Write("Enter number of node layers (excluding input nodes) [Integer]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!int.TryParse(userInput, out numLayers));

            //Obtains number of nodes for each of the layers
            nodesPerLayer = new int[numLayers];
            for (int i = 0; i < numLayers; i++) {
                inputTries = 0;
                do
                {
                    if (inputTries > 0) Console.WriteLine("Failed to obtain an integer from input, please try again.");
                    Console.Write("Enter number of nodes for layer " + (i+1) + " of nodes [Integer]: ");
                    inputTries++;
                    userInput = Console.ReadLine();
                } while (!int.TryParse(userInput, out nodesPerLayer[i]));
            }

            //Obtains learning rate
            inputTries = 0;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain a float from input, please try again.");
                Console.Write("Enter the learning rate [Double]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!double.TryParse(userInput, out lrCurr));

            //Obtains absolute error
            inputTries = 0;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain a float from input, please try again.");
                Console.Write("Enter the absolute error [Double]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!double.TryParse(userInput, out absErr));

            //Obtains weight momentum
            inputTries = 0;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain a float from input, please try again.");
                Console.Write("Enter weight momentum [Double]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!double.TryParse(userInput, out weightMomentum));

            //Obtains number of tests per epoch
            inputTries = 0;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain an integer from input, please try again.");
                Console.Write("Enter number of tests per epoch [Integer]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!int.TryParse(userInput, out testsPerEpoch));

            //Obtains the input values lists
            //TODO: Make the input values file selectable by the user
            int success = ObtainInputNodeInfo(@"NeuralTests/BaseTest.txt");
            if (success != 0)
            {
                Console.WriteLine("Process of obtaining input values has failed. See output above for more info.");
                return -1;
            }

            //Obtains the test answers
            //TODO: Make the test answers file selectable by the user
            success = ObtainTestAnswers(@"NeuralTests/BaseTestAnswers.txt");
            if (success != 0)
            {
                Console.WriteLine("Process of obtaining test answers has failed. See output above for more info.");
                return -1;
            }

            for (int i = 0; i < testsPerEpoch; i++)
            {
                List<double> testInputs = new List<double>();
                for (int j = 0; j < inputValuesLists.Count(); j++)
                {
                    testInputs.Add(inputValuesLists[j][i % inputValuesLists[j].Count()]);
                }
                inputValuesForTests.Add(testInputs);
            }
            return 0;
        }

        //Dedicated to obtaining the info for inputValuesLists
        public int ObtainInputNodeInfo(string inputFile)
        {
            Console.WriteLine("Trying to obtain input");
            if (System.IO.File.Exists(inputFile))
            {
                Console.WriteLine("File opened");
                using (TextReader reader = System.IO.File.OpenText(inputFile))
                {
                    for (int i = 0; i < numInputs; i++) {
                        string text = reader.ReadLine();
                        if (text != null) {
                            List<double> temp = new List<double>();
                            string[] bits = text.Split('\t');
                            for (int j = 0; j < bits.Length; j++)
                            {
                                double x = double.Parse(bits[j]);
                                temp.Add(x);
                            }
                            inputValuesLists.Add(temp);
                        } else
                        {
                            Console.WriteLine("No input for a possible input node " + (i+1) + " as end of file was reached instead");
                            return -1;
                        }
                    }
                }
            } else
            {
                Console.WriteLine("File failed to open");
                return -1;
            }
            foreach(List<double> inputList in inputValuesLists)
            {
                foreach (double val in inputList)
                {
                    Console.Write(val + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("All input values have been obtained which are listed above");
            return 0;
        }

        //Dedicated to obtaining the info for testAnswers
        public int ObtainTestAnswers(string inputFile)
        {
            Console.WriteLine("Trying to obtain test answers");
            if (System.IO.File.Exists(inputFile))
            {
                Console.WriteLine("File opened");
                using (TextReader reader = System.IO.File.OpenText(inputFile))
                {
                    string text = reader.ReadLine();
                    if (text != null)
                    {
                        string[] bits = text.Split('\t');
                        for (int j = 0; j < bits.Length; j++)
                        {
                            double x = double.Parse(bits[j]);
                            testAnswers.Add(x);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No test answers as end of file was reached instead");
                        return -1;
                    }
                }
            }
            else
            {
                Console.WriteLine("File failed to open");
                return -1;
            }
            foreach (double val in testAnswers)
            {
                Console.Write(val + " ");
            }
            Console.WriteLine();
            Console.WriteLine("All answers have been obtained which are listed above");
            return 0;
        }

        //Generates the Neural Network
        public void GenerateNetwork()
        {
            //The base input nodes are created
            for (int i = 0; i < numInputs; i++)
            {
                lastInputLayer.Enqueue((new InputNode(inputValuesLists[i]), RndWeightBias()));
            }
            allNodesAndInputs.Push(lastInputLayer);

            //Now create the all the learning nodes
            for(int i = 0; i < numLayers; i++)
            {
                currWorkingLayer = new Queue<(NeuralNode, double)>();
                for (int j = 0; j < nodesPerLayer[i]; j++)
                {
                    int nodesInLastLayer = lastInputLayer.Count;
                    for (int k = 0; k < nodesInLastLayer; k++)
                    {
                        (NeuralNode, double) node = lastInputLayer.Dequeue();
                        node.Item2 = RndWeightBias();
                        lastInputLayer.Enqueue(node);
                    }
                    currWorkingLayer.Enqueue((new LearnableNode(new Queue<(NeuralNode, double)>(lastInputLayer), RndWeightBias()), RndWeightBias()));
                }
                //Move on to the next layer
                allNodesAndInputs.Push(currWorkingLayer);
                lastInputLayer = currWorkingLayer;
            }
            //Select output node to start learning operation on
            outputNode = lastInputLayer.Peek().Item1;
            PresentNodeNetwork();
        }

        public void RunTraining()
        {
            epochs = 0;
            List<List<double>> testResults = new List<List<double>>();
            //using StreamWriter file = System.IO.File.CreateText(@"C:\Users\alsro\Source\Repos\MLP_Lab1\MLP_Lab1\NeuralTests\TrainingOutput.txt");
            Console.WriteLine("Running training");
            List<int> testingOrder;
            do
            {
                testingOrder = new List<int>();
                for (int i = 0; i < testsPerEpoch; i++)
                {
                    testingOrder.Add(i);
                }
                testingOrder = Shuffle(testingOrder);
                epochs++;
                //file.WriteLine("Outputs and Errors for Epoch " + (epochs + 1) + ":");
                testResults = RunEpoch(testingOrder);
            } while (!CheckTraining(testResults[1]));

            //Exit training as it has finished successfully
            Console.WriteLine("Training has finished successfully!");
            Console.WriteLine("Expected outputs for Epoch " + epochs + ": ");
            foreach (int val in testingOrder)
            {
                Console.Write(" " + testAnswers[val]);
            }
            Console.WriteLine();
            Console.WriteLine("Outputs for Epoch " + epochs + ": ");
            foreach (double val in testResults[0])
            {
                Console.Write(" " + Math.Truncate(val * 1000) / 1000);
            }
            Console.WriteLine();
            Console.WriteLine("Errors for Epoch " + epochs + ": ");
            foreach (double val in testResults[1])
            {
                Console.Write(" " + Math.Truncate(val * 1000) / 1000);
            }
            Console.WriteLine();

        }

        //Used to determine if the training has reached the required error range
        public bool CheckTraining(List<double> testErrs)
        {
            foreach(double err in testErrs)
            {
                //Console.WriteLine("Test Error was... " + err);
                if (err.CompareTo(absErr) >= 0) return false;
            }
            return true;
        }

        //Runs an epoch, returns the absolute error values for each test that was run in list form
        public List<List<double>> RunEpoch(List<int> testingOrder)
        {
            List<double> testErrs = new List<double>();
            List<double> testOuts = new List<double>();

            foreach (int i in testingOrder)
            {
                //Get the output for the test
                double output = outputNode.NodeOutput(i);

                //Add the output to the list of outputs for the epoch
                testOuts.Add(output);
                //file.WriteLine("Output for Test " + i + " is " + output + ". The expected output was " + testAnswers[i] + ".");
                
                //Train the initial output node
                double tempLR = lrCurr;
                if (weightMomentum != 1) tempLR = lrCurr / (1 - weightMomentum);
                Stack<Queue<(NeuralNode, double)>> temp = new Stack<Queue<(NeuralNode, double)>>();
                temp.Push(allNodesAndInputs.Pop());
                double outputd = testAnswers[i] - output;
                testErrs.Add(Math.Abs(outputd));
                ((LearnableNode)outputNode).OutToLearnFrom(outputd);
                ((LearnableNode)outputNode).BackTrackLearn(i, tempLR);

                //Train the hidden layers
                while (allNodesAndInputs.Count() > 0) {
                    Queue<(NeuralNode, double)> layer = allNodesAndInputs.Pop();
                    //Check to see if the layer is actual learning nodes
                    if (layer.Peek().Item1 is LearnableNode) {
                        //Train the learning nodes
                        for (int k = 0; k < layer.Count(); k++)
                        {
                            (NeuralNode, double) node = layer.Dequeue();
                            ((LearnableNode)(node.Item1)).BackTrackLearn(i, tempLR);
                            layer.Enqueue(node);
                        }
                    }
                    temp.Push(layer);
                }
                while (temp.Count() > 0)
                {
                    allNodesAndInputs.Push(temp.Pop());
                }
            }

            if (epochs % 100000 == 0)
            {
                Console.WriteLine("Expected outputs for Epoch " + epochs + ": ");
                foreach (int val in testingOrder)
                {
                    Console.Write(" " + testAnswers[val]);
                }
                Console.WriteLine();
                Console.WriteLine("Outputs for Epoch " + epochs + ": ");
                foreach (double val in testOuts)
                {
                    Console.Write(" " + Math.Truncate(val * 1000) / 1000);
                }
                Console.WriteLine();
                Console.WriteLine("Errors for Epoch " + epochs + ": ");
                foreach (double val in testErrs)
                {
                    Console.Write(" " + Math.Truncate(val * 1000) / 1000);
                }
                Console.WriteLine();
                PresentNodeNetwork();
                Console.WriteLine();
            }
            List<List<double>> errOuts = new List<List<double>>();
            errOuts.Add(testOuts);
            errOuts.Add(testErrs);
            //System.Threading.Thread.Sleep(1000);
            return errOuts;
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

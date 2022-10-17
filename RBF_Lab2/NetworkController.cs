using System;
using System.IO;
using System.Text;
using static System.Net.WebRequestMethods;

namespace RBF_Lab2
{
    public class NetworkController
    {
        int numEpochs, testsPerEpoch, numGauCenters;
        bool oneGauWidth;
        double learnRate;
        Stack<Queue<(NeuralNode, double)>> allNodesAndInputs;
        Queue<(NeuralNode, double)> lastInputLayer, currWorkingLayer;
        NeuralNode outputNode;
        List<(double, double)> generatedPoints;
        List<(double, double)> centersAndWidths = new List<(double, double)>();
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

            //Obtains whether to do seperate or same gau widths for each center
            inputTries = 0;
            int doGausWidths;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain an integer from input, please try again.");
                Console.Write("Enter 0 for seperate gaussian widths, or any other integer for same gaussian widths [Integer]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!int.TryParse(userInput, out doGausWidths));
            if (doGausWidths == 0) { oneGauWidth = false; } else { oneGauWidth = true; }
        }

        //Generates the Neural Network
        public void GenerateNetwork()
        {
            //Generate the list of random points
            Console.WriteLine("Generating points...");
            GeneratePoints();

            //The base input node is created
            lastInputLayer.Enqueue((new InputNode(generatedPoints), 1.0));
            allNodesAndInputs.Push(lastInputLayer);

            //Generate the centers and the widths for each center
            Console.WriteLine("Generating centers and widths...");
            KMeans();

            //Now create the all the hidden nodes
            Console.WriteLine("Creating hidden nodes...");
            currWorkingLayer = new Queue<(NeuralNode, double)>();
            for (int j = 0; j < numGauCenters; j++)
            {
                currWorkingLayer.Enqueue((new RBFHiddenNode(lastInputLayer.Peek().Item1, centersAndWidths[j]), RndWeightBias()));
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

        //Generates the k-means centers for the number of training points
        private void KMeans()
        {
            //Selection of centers
            List<double> centerChanges = new List<double>();
            List<double> centers = new List<double>();
            List<List<double>> pointsForCenters = new List<List<double>>();
            for (int selection = 0; selection < numGauCenters; selection++)
            {
                centers.Add(generatedPoints[selection].Item1);
            }

            bool noCenterChange;
            do
            {
                pointsForCenters.Clear();
                for (int i = 0; i < numGauCenters; i++)
                {
                    pointsForCenters.Add(new List<double>());
                }
                //Assign the points to the centers
                for (int genPoint = 0; genPoint < generatedPoints.Count; genPoint++)
                {
                    (int, double) currCenter;
                    currCenter = (0, generatedPoints[genPoint].Item1 - centers[0]);
                    for (int center = 1; center < centers.Count; center++)
                    {
                        double nextDist = generatedPoints[genPoint].Item1 - centers[center];
                        if (nextDist * nextDist < currCenter.Item2 * currCenter.Item2 || (currCenter.Item2 * currCenter.Item2).Equals(nextDist * nextDist))
                        {
                            currCenter = (center, nextDist);
                        }
                    }
                    pointsForCenters[currCenter.Item1].Add(generatedPoints[genPoint].Item1);
                }

                //Update centers
                centerChanges.Clear();
                for (int i = 0; i < numGauCenters; i++)
                {
                    centerChanges.Add(0);
                }
                for (int center = 0; center < centers.Count; center++)
                {
                    double xSum = 0;
                    foreach (double xVal in pointsForCenters[center])
                    {
                        xSum += xVal;
                    }
                    double newCenter = (1f / (double)pointsForCenters[center].Count) * xSum;
                    centerChanges[center] = newCenter - centers[center];
                    centers[center] = newCenter;
                }

                //Check for center changes
                noCenterChange = true;
                foreach (double centerChange in centerChanges)
                {
                    //Console.Write(centerChange);
                    if (!centerChange.Equals(0)) noCenterChange = false;
                    break;
                }
                //Console.WriteLine();
            } while (!noCenterChange);
            List<double> gauWidths = GauWidths(centers, pointsForCenters);

            for (int i = 0; i < centers.Count; i++)
            {
                centersAndWidths.Add((0, 0));
                if (!oneGauWidth) {
                    centersAndWidths[i] = (centers[i], gauWidths[i]);
                } else {
                    centersAndWidths[i] = (centers[i], gauWidths[0]);
                }
            }
        }

        private List<double> GauWidths(List<double> centers, List<List<double>> pointsForCenters)
        {
            switch (oneGauWidth)
            {
                case (true):
                    Console.WriteLine("Doing the width, only ONE width!");
                    double distBtwnCenters = 0;
                    for (int leftCenter = 0; leftCenter < centers.Count; leftCenter++)
                    {
                        for (int rightCenter = leftCenter + 1; rightCenter < centers.Count; rightCenter++)
                        {
                            double centerDist = centers[leftCenter] - centers[rightCenter];
                            if (Math.Abs(centerDist) > distBtwnCenters) distBtwnCenters = centerDist;
                        }
                    }
                    List<double> gauCenter = new List<double> { distBtwnCenters / Math.Sqrt(2f * (double)numGauCenters) };
                    foreach(double center in gauCenter)
                    {
                        Console.WriteLine("The gaussian center is " + center);
                    }
                    return gauCenter;
                case (false):
                    List<double> gauWidths = new List<double>();
                    for (int i = 0; i < centers.Count; i++)
                    {
                        gauWidths.Add(0);
                    }
                    for (int center = 0; center < centers.Count; center++)
                    {
                        double pointVarSum = 0;
                        foreach (double xVal in pointsForCenters[center])
                        {
                            pointVarSum += (centers[center] - xVal) * (centers[center] - xVal);
                        }
                        gauWidths[center] = (1f / (double)pointsForCenters.Count) * pointVarSum;
                    }
                    return gauWidths;
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
                Console.WriteLine(generatedPoints[val].Item1);
                testNum++;
            }
            Console.WriteLine();
            foreach (int val in testingOrder)
            {
                Console.WriteLine(generatedPoints[val].Item2);
                testNum++;
            }
            /*Console.WriteLine("///////////////////////////////////////////////////////////////////////////////////");
            Console.WriteLine("Produced points in test order for Epoch " + numEpochsRun + ": ");
            for (int i = 1; i <= testsPerEpoch; i++)
            {
                Console.WriteLine(testResults[i - 1].Item1);
            }
            Console.WriteLine();
            for (int i = 1; i <= testsPerEpoch; i++)
            {
                Console.WriteLine(testResults[i - 1].Item2);
            }*/
            Console.WriteLine("///////////////////////////////////////////////////////////////////////////////////");
            PresentNodeNetwork();
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
                ((LearnableNode)outputNode).RBFLearn(i, learnRate, outputd);     //Perform learning on the output node
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

                Console.WriteLine(((RBFHiddenNode)node.Item1).GetGauWidth());
                learnNodeNum++;
            }
            Console.WriteLine();
            foreach ((NeuralNode, double) node in ((LearnableNode)outputNode).GetInputs())
            {

                Console.WriteLine(((RBFHiddenNode)node.Item1).GetCenter());
                learnNodeNum++;
            }
            Console.WriteLine();
            foreach ((NeuralNode, double) node in ((LearnableNode)outputNode).GetInputs())
            {

                Console.WriteLine(node.Item2);
                learnNodeNum++;
            }
            Console.WriteLine();
        }
    }
}

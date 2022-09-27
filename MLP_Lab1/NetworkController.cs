// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Text;

namespace MLP_Lab1
{
    public class NetworkController
    {
        int numInputs, numLayers, epochs;
        int[] nodesPerLayer;
        float lrChange, weightMomentum, lrCurr, absErr;
        float[] lrBounds;
        Queue<(NeuralNode, float)> lastInputLayer, currWorkingLayer;
        NeuralNode outputNode;
        List<List<int>> inputValuesLists;

        public NetworkController()
        {
            inputValuesLists = new List<List<int>>();
        }

        //Will obtain the info to fill in numInputs, numLayers, nodesPerLayer, lrChange, weightMomentum, lrBounds, and inputValuesLists
        public void ObtainBaseInfo()
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
            //Console.WriteLine("Obtained integer was " + numInputs);

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

            //Obtains learning rate change rate
            inputTries = 0;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain a float from input, please try again.");
                Console.Write("Enter rate of change for the learning rate [Float]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!float.TryParse(userInput, out lrChange));

            //Obtains learning rate start and end
            lrBounds = new float[2];
            inputTries = 0;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain a float from input, please try again.");
                Console.Write("Enter learning rate start [Float]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!float.TryParse(userInput, out lrBounds[0]));

            inputTries = 0;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain a float from input, please try again.");
                Console.Write("Enter learning rate end [Float]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!float.TryParse(userInput, out lrBounds[1]));

            //Obtains weight momentum
            inputTries = 0;
            do
            {
                if (inputTries > 0) Console.WriteLine("Failed to obtain a float from input, please try again.");
                Console.Write("Enter weight momentum [Float]: ");
                inputTries++;
                userInput = Console.ReadLine();
            } while (!float.TryParse(userInput, out weightMomentum));

            //Obtains the input values lists
            ObtainInputNodeInfo(@"NeuralTests/BaseTest.txt");
        }

        //Dedicated to obtaining the info for inputValuesLists
        public void ObtainInputNodeInfo(string inputFile)
        {
            Console.WriteLine("Trying to obtain input");
            if (File.Exists(inputFile))
            {
                Console.WriteLine("File opened");
                using (TextReader reader = File.OpenText(inputFile))
                {
                    for (int i = 0; i < 6; i++) {
                        string text = reader.ReadLine();
                        if (text != null) {
                            List<int> temp = new List<int>();
                            string[] bits = text.Split('\t');
                            for (int j = 0; j < bits.Length; j++)
                            {
                                int x = int.Parse(bits[j]);
                                temp.Add(x);
                            }
                            inputValuesLists.Add(temp);
                        } else
                        {
                            Console.WriteLine("No input for a possible input node " + (i+1) + " as end of file was reached instead");
                        }
                    }
                }
            } else
            {
                Console.WriteLine("File failed to open");
            }
            foreach(List<int> inputList in inputValuesLists)
            {
                foreach (int val in inputList)
                {
                    Console.Write(val + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("All input values have been obtained which are listed above");
        }
    }
}

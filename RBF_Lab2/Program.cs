using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBF_Lab2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            String userInput;
            do
            {
                NetworkController networkController = new NetworkController();
                Console.WriteLine("Getting info...");
                networkController.ObtainBaseInfo();
                Console.WriteLine("Generating network...");
                networkController.GenerateNetwork();
                Console.WriteLine("Running training...");
                networkController.RunTraining();
                Console.Write("Would you like to run another training? [y/n]: ");
                userInput = Console.ReadLine();
            } while (userInput != null && userInput.Equals("y"));
        }
    }
}

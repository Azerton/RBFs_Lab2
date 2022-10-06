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
                int success = networkController.ObtainBaseInfo();
                if (success == 0)
                {
                    networkController.GenerateNetwork();
                    networkController.RunTraining();
                }
                Console.Write("Would you like to run another training? [y/n]: ");
                userInput = Console.ReadLine();
            } while (userInput != null && userInput.Equals("y"));
        }
    }
}

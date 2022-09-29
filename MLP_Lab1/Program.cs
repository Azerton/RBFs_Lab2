using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLP_Lab1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            NetworkController networkController = new NetworkController();
            int success = networkController.ObtainBaseInfo();
            if (success == 0) {
                networkController.GenerateNetwork();
                networkController.RunTraining();
            }
        }
    }
}

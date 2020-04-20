using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITF_PC_Service_Configurator
{
    class Program
    {
        static void Main(string[] args)
        {
            byte desiredTemperature = 0;
            while (true)
            {
                byte inputValue;
                Console.Write("Desired temperature: ");
                string userInput = Console.ReadLine();
                try
                {
                    inputValue = Convert.ToByte(userInput);
                    if (inputValue > 80)
                    {
                        Console.WriteLine("The temperature value of {0} is too high, do you want to damage the system?", inputValue);
                    }
                    else
                    {
                        desiredTemperature = inputValue;
                        UDPSend.sendTemp(desiredTemperature);
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Inputvalue is not between 0 and 80");
                }
            }
        }
    }
}

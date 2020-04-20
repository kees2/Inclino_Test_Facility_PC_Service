using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace UDP_Test
{
    class Program
    {


        static void Main(string[] args)
        {


            DataParser.initDataHandler();

            while (true)
            {
                Thread.Sleep(10000000);
            }
        }
    }   
}

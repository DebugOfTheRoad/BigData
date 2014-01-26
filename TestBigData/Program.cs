using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBigData
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = new TestRunner();
            runner.RunAll();
            foreach (var line in runner.Output)
            {
                Console.WriteLine(line);
            }
            Console.ReadLine();
        }
    }
}

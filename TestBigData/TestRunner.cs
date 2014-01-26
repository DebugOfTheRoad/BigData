using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestBigData
{
    public class TestRunner
    {
        public void RunAll()
        {
            var super = Type.GetType("TestBigData.Test");
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(super));
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name.StartsWith("Test"));

                foreach (var test in methods)
                {
                    dynamic inst = Activator.CreateInstance(type);
                    Log("# " + test.Name);
                    test.Invoke(inst, null);

                    Log("## " + inst.Result);
                    if (Result.Fail == inst.Result)
                    {
                        Log("### " + inst.Reason);
                    }
                }
            }
        }

        public TestRunner()
        {
            output = new List<string>();
        }

        private List<string> output;
        public List<string> Output
        {
            get { return output; }
        }

        private void Log(string message)
        {
            output.Add(message);
        }
    }
}

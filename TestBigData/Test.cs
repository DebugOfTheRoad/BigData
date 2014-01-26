using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestBigData
{
    public abstract class Test
    {
        private Result result;
        public Result Result
        {
            get { return result; }
        }

        private string reason;
        public string Reason
        {
            get { return reason; }
        }

        public Test()
        {
            result = Result.Pending;
        }

        public void Pass()
        {
            result = Result.Pass;
        }

        public void Fail(string message)
        {
            reason = message;
            result = Result.Fail;
        }
    }

    public enum Result
    {
        Pass, Fail, Pending
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class TestTest : Test
    {
        public void TestFail()
        {
            Fail("Not Implemented");
        }

        public void TestPass()
        {
            Pass();
        }

        public void TestPending()
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BigData;

namespace TestBigData
{
    class TestAPI : Test
    {
        public void TestPublicationCreation()
        {
            // This url points to the RSS feed for the list
            var listURL = "https://bucknell.worldcat.org/profiles/danieleshleman/lists/3234701/rss";
            OCLCWrapper wrapper = new OCLCWrapper(listURL);
            //System.Console.WriteLine(wrapper.getXML());
            List<Publication> list = wrapper.createPublications();
            if (!list.Any())
            {
                Fail("No valid publications found");
            }
            else
            {
                Pass(); //#All the data
            }
        }
    }
}

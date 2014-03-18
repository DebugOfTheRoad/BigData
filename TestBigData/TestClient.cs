using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBigData
{
    class TestClient : Test
    {
        public void TestFetchPublicationsFromRSS()
        {
            var client = new BigData.OCLC.Client(
                @"XYBOEZiodAgSpDI9gLiQcv6o4r78ZuHOELWT2c7F5F9iqIx7VXnbXrt4a2HTpUYCDSKOwoD25joHpkVy");

            var publications = client.FetchPublicationsFromRSS
                (@"https://bucknell.worldcat.org/profiles/danieleshleman/lists/3234701/rss");

            foreach (var p in publications)
            {
                Console.WriteLine(p);
            }
        }
    }
}

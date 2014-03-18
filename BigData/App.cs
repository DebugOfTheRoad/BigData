using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BigData
{
    class App : Application
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var a = new App();
            a.MainWindow = new MainWindow();
            a.Run();
        }

        public static OCLC.Client GetOCLCClient()
        {
            return new OCLC.Client(@"XYBOEZiodAgSpDI9gLiQcv6o4r78ZuHOELWT2c7F5F9iqIx7VXnbXrt4a2HTpUYCDSKOwoD25joHpkVy");
        }

        public const string PublicationListUri = @"https://bucknell.worldcat.org/profiles/danieleshleman/lists/3234701/rss";
    }
}

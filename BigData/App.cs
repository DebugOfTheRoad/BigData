using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BigData {
    class App : Application {
        [STAThread]
        public static void Main(string[] args) {
            var a = new App();
            a.MainWindow = new UI.MainWindow();
            a.Run();
        }
    }
}

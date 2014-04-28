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
            try {
                var a = new App();
                var server = new Management_Interface.ManagementServer();
                server.CreateServer();
                a.MainWindow = new UI.MainWindow();
                a.Run();
                server.StopServer();
            } catch (Exception ex) {
                MessageBox.Show(ex.StackTrace, ex.Message);
            }

        }
    }
}

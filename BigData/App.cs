using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

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

        public App() {
            var timer = new DispatcherTimer {
                Interval = TimeSpan.FromHours(24)
            };
            timer.Tick += delegate {
                if (Source != null && Source.Callback != null) {
                    Source.Callback();
                }
            };
            timer.Start();
        }

        public OCLC.Database Source { get; set; }
    }
}

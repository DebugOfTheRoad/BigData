using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BigData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double scrollOffset;
        private Rectangle[] artwork;

        public MainWindow()
        {
            InitializeComponent();
            image.Source = RenderArtwork();
        }

        private ImageSource RenderArtwork()
        {
            var group = new DrawingGroup();
            var catcher = new BitmapImage(
                new Uri(@"pack://application:,,,/Resources/catcher.jpg", UriKind.RelativeOrAbsolute)
            );

            for (var i = 0; i < 10; i++)
            {
                group.Children.Add(new ImageDrawing(
                    catcher,
                    new Rect(i * 200, 0, 200, 300)
                ));
            }

            var source = new DrawingImage(group);
            source.Freeze();
            return source;
        }

        private void ReDraw(object sender, MouseEventArgs args)
        {
            //if (args.LeftButton == MouseButtonState.Released) return;
            scrollOffset = args.GetPosition(this).X;
            var m = new Thickness();
            m.Left = scrollOffset;
            image.Margin = m;
            
        }

        private void Exit(object sender, KeyEventArgs args)
        {
            Close();
        }
    }
}

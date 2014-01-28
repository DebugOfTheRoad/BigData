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

        public MainWindow()
        {
            InitializeComponent();

            scrollOffset = 0;
            offsetStart = 0;
            dragStart = 0;

            image.Source = RenderArtwork();
            image2.Source = RenderArtwork();
        }

        private ImageSource RenderArtwork()
        {
            var group = new DrawingGroup();
            var catcher = new BitmapImage(
                new Uri(@"pack://application:,,,/Resources/catcher.jpg", UriKind.RelativeOrAbsolute)
            );

            double imageHeight = 300;
            double imageWidth = (imageHeight / catcher.PixelHeight) * catcher.PixelWidth;

            for (var i = 0; i < 10; i++)
            {
                group.Children.Add(new ImageDrawing(
                    catcher,
                    new Rect(i * imageWidth, 0, imageWidth, imageHeight)
                ));
                group.Children.Add(new ImageDrawing(
                    catcher,
                    new Rect(i * imageWidth, imageHeight, imageWidth, imageHeight)
                ));
                group.Children.Add(new ImageDrawing(
                    catcher,
                    new Rect(i * imageWidth, imageHeight * 2, imageWidth, imageHeight)
                ));
            }

            var source = new DrawingImage(group);
            source.Freeze();
            return source;
        }

        private double dragStart;
        private double offsetStart;
        private void OnMouseDown(object sender, MouseEventArgs args)
        {
            dragStart = args.GetPosition(this).X;
            offsetStart = scrollOffset;
        }

        private void ReDraw(object sender, MouseEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Released) return;
            scrollOffset = args.GetPosition(this).X - dragStart + offsetStart;
            var m = new Thickness();
            m.Left = scrollOffset;
            image.Margin = m;

            var n = new Thickness();
            n.Left = scrollOffset - image2.Source.Width;
            image2.Margin = n;
        }

        private void Exit(object sender, KeyEventArgs args)
        {
            Close();
        }
    }
}

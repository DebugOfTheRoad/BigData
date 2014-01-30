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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BigData
{
    public class MainWindow : Window
    {
        private Canvas canvas;
        private Image image;
        private Storyboard storyboard;
        private DoubleAnimation animation;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;
            this.Visibility = System.Windows.Visibility.Visible;
            this.Title = "Digital Publication Display";
            this.KeyUp += this.OnKeyUp;
            this.MouseMove += this.SeekToMouse;

            canvas = new Canvas();
            this.Content = canvas;

            image = new Image();
            image.Source = RenderArtwork();
            canvas.Children.Add(image);

            NameScope.SetNameScope(this, new NameScope());
            image.RenderTransform = new TranslateTransform();
            RegisterName("transform", image.RenderTransform);

            animation = new DoubleAnimation(1440, new Duration(TimeSpan.FromSeconds(20)));
            Storyboard.SetTargetName(animation, "transform");
            Storyboard.SetTargetProperty(animation, new PropertyPath(TranslateTransform.XProperty));

            storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin(this, true);
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

        private void OnClick(object sender, MouseEventArgs args)
        {
            storyboard.Pause(this);
        }

        private void SeekToMouse(object sender, MouseEventArgs args)
        {
            storyboard.Pause(this);

            var percent = args.GetPosition(this).X / this.Width;
            var seekTo = percent * animation.Duration.TimeSpan.TotalSeconds;
            storyboard.SeekAlignedToLastTick(this, TimeSpan.FromSeconds(seekTo), TimeSeekOrigin.BeginTime);
        }

        private void OnKeyUp(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Q) Close();
        }
    }
}

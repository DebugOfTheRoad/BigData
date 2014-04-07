using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private Canvas canvas;
        private double tileWidth;
        private bool isMouseDown;
        private Dictionary<Image, double> startingPositions;
        private double dragBegin;
        private double scrollVelocity;
        private double lastMousePosition;

        private void InitializeComponent()
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Visibility = Visibility.Visible;
            Title = "Digital Publication Display";

            KeyUp += OnKeyUp;
            Loaded += PopulateDisplay;
            MouseDown += BeginMouseTracking;
            MouseUp += EndMouseTracking;
            MouseMove += TrackMouse;

            canvas = new Canvas();
            canvas.RenderTransform = new TranslateTransform() { X = -500 };
            Content = canvas;

            isMouseDown = false;
            scrollVelocity = 0.5;

            CompositionTarget.Rendering += ScrollImages;
            CompositionTarget.Rendering += UpdateScrollVelocity;
        }

        private void TrackMouse(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double translation = e.GetPosition(this).X - dragBegin;
                foreach (var pair in startingPositions)
                {
                    var image = pair.Key;
                    var position = pair.Value;

                    var newPosition = (position + translation) % tileWidth;
                    if (newPosition < 0) newPosition += tileWidth;
                    Canvas.SetLeft(image, newPosition);
                }

                double thisPosition = e.GetPosition(this).X;
                var timer = new Timer(16);
                timer.Elapsed += (s, a) =>
                {
                    lastMousePosition = thisPosition;
                    timer.Stop();
                };
                timer.Start();
            }
        }

        private void EndMouseTracking(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
            scrollVelocity = e.GetPosition(this).X - lastMousePosition;
        }

        private void BeginMouseTracking(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            dragBegin = e.GetPosition(this).X;
            lastMousePosition = dragBegin;
            startingPositions = canvas.Children.OfType<Image>().ToDictionary(
                im => im,
                im => Canvas.GetLeft(im)
            );
        }

        void ScrollImages(object sender, EventArgs e)
        {
            if (!isMouseDown)
            {
                foreach (var child in canvas.Children)
                {
                    var image = (Image)child;
                    var nextX = (Canvas.GetLeft(image) + scrollVelocity) % tileWidth;
                    Canvas.SetLeft(image, nextX);
                }
            }
        }

        void UpdateScrollVelocity(object sender, EventArgs e)
        {
            if (Math.Abs(scrollVelocity) > 0.5)
            {
                scrollVelocity *= 0.90;
            }
            else if (scrollVelocity > 0)
            {
                scrollVelocity = 0.5;
            }
            else if (scrollVelocity < 0)
            {
                scrollVelocity = -0.5;
            }
        }

        async void PopulateDisplay(object sender, RoutedEventArgs e)
        {
            //OCLC.PublicationSource src = new OCLC.Client(Properties.Settings.Default.WSKey, Properties.Settings.Default.RSSUri);
            //OCLC.PublicationSource src = new OCLC.Cache(@"C:\Users\davis\Documents\GitHub\BigData\BigData\bin\Debug\cache.dat");
            var src = new OCLC.Database(Properties.Settings.Default.WSKey, Properties.Settings.Default.RSSUri);
            await src.createDatabase();
            //await src.updateDatabase();
            var publications = await src.GetPublications();

            var images = (from pub in publications
                          select new Image() { Source = pub.CoverImage, Height = 300 }).ToArray();
            tileWidth = images.Aggregate(0.0, (acc, im) => acc + (im.Height / im.Source.Height) * im.Source.Width);


            double offset = 0;
            foreach (var image in images)
            {
                canvas.Children.Add(image);
                Canvas.SetLeft(image, offset);
                offset += (image.Height / image.Source.Height) * image.Source.Width;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Q) Close();
        }
    }
}

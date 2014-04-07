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
        private WrappingCollectionView[] views;
        private int activeViewIndex;

        private void InitializeComponent()
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Visibility = Visibility.Visible;
            Title = "Digital Publication Display";

            KeyUp += OnKeyUp;
            Loaded += PopulateDisplay;

            canvas = new Canvas();
            Content = canvas;

            MouseDown += (sender, e) =>
            {
                activeViewIndex = (int)(e.GetPosition(this).Y * 3 / this.Height);
                views[activeViewIndex].BeginTouchTracking(e.GetPosition(this).X);
            };
            TouchDown += (sender, e) =>
            {
                activeViewIndex = (int)(e.GetTouchPoint(this).Position.Y % (this.Height / 3));
                views[activeViewIndex].BeginTouchTracking(e.GetTouchPoint(this).Position.X);
            };

            MouseUp += (sender, e) =>
            {
                views[activeViewIndex].EndTouchTracking(e.GetPosition(this).X);
            };
            TouchUp += (sender, e) =>
            {
                views[activeViewIndex].EndTouchTracking(e.GetTouchPoint(this).Position.X);
            };

            MouseMove += (sender, e) =>
            {
                views[activeViewIndex].TrackTouch(e.GetPosition(this).X);
            };
            TouchMove += (sender, e) =>
            {
                views[activeViewIndex].TrackTouch(e.GetTouchPoint(this).Position.X);
            };
        }

        async void PopulateDisplay(object sender, RoutedEventArgs e)
        {
            //OCLC.PublicationSource src = new OCLC.Client(Properties.Settings.Default.WSKey, Properties.Settings.Default.RSSUri);
            //OCLC.PublicationSource src = new OCLC.Cache(@"C:\Users\davis\Documents\GitHub\BigData\BigData\bin\Debug\cache.dat");
            var src = new OCLC.Database(Properties.Settings.Default.WSKey, Properties.Settings.Default.RSSUri);
            await src.createDatabase();
            var publications = await src.GetPublications();

            var allImages = (from pub in publications
                             select new Image() { Source = pub.CoverImage, Height = this.Height / 3 }).ToArray();

            var imagesPerRow = allImages.Length / 3;

            views = new WrappingCollectionView[3];

            views[0] = new WrappingCollectionView(allImages.Take(imagesPerRow).ToArray());
            canvas.Children.Add(views[0]);

            views[1] = new WrappingCollectionView(allImages.Skip(imagesPerRow).Take(imagesPerRow).ToArray());
            canvas.Children.Add(views[1]);
            Canvas.SetTop(views[1], Height / 3);

            views[2] = new WrappingCollectionView(allImages.Skip(imagesPerRow * 2).Take(imagesPerRow).ToArray());
            canvas.Children.Add(views[2]);
            Canvas.SetTop(views[2], 2 * Height / 3);
        }

        private void OnKeyUp(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Q) Close();
        }
    }
}

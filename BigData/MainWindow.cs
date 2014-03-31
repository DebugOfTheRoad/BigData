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
        private Storyboard storyboard;
        private DoubleAnimation animation;
        private double mouseDragStart;
        private double imageDragStart;

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
            this.MouseUp += this.RestartAnimation;
            this.MouseDown += this.OnClick;
            this.TouchDown += MainWindow_TouchDown;
            this.TouchMove += MainWindow_TouchMove;
            this.Loaded += MainWindow_Loaded;
        }

        async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //OCLC.PublicationSource src = new OCLC.Client(Properties.Settings.Default.WSKey, Properties.Settings.Default.RSSUri);
            //OCLC.PublicationSource src = new OCLC.Cache(@"C:\Users\davis\Documents\GitHub\BigData\BigData\bin\Debug\cache.dat");
            var src = new OCLC.Database(Properties.Settings.Default.WSKey, Properties.Settings.Default.RSSUri);
            //src.create_db();
            //await src.update_db();
            var publications = await src.GetPublications();

            var images = from pub in publications
                         select new Image { Source = pub.CoverImage };

            var canvas = new ImageCollectionCanvas() { TileWidth = this.Width * 2, Images = images.ToArray() };
            this.Content = canvas;

            NameScope.SetNameScope(this, new NameScope());
            canvas.RenderTransform = new TranslateTransform();
            RegisterName("transform", canvas.RenderTransform);

            animation = new DoubleAnimation(0, this.Width, new Duration(TimeSpan.FromSeconds(5)));
            animation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(animation, "transform");
            Storyboard.SetTargetProperty(animation, new PropertyPath(TranslateTransform.XProperty));

            storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin(this, true);
        }

        void MainWindow_TouchMove(object sender, TouchEventArgs e)
        {
            storyboard.Pause(this);

            var percent = (e.GetTouchPoint(this).Position.X - mouseDragStart + imageDragStart) / this.Width;
            var seekTo = percent * animation.Duration.TimeSpan.TotalSeconds;
            if (seekTo < 0) { seekTo = 0; }
            storyboard.SeekAlignedToLastTick(this, TimeSpan.FromSeconds(seekTo), TimeSeekOrigin.BeginTime);
        }

        void MainWindow_TouchDown(object sender, TouchEventArgs e)
        {
            storyboard.Pause(this);
            mouseDragStart = e.GetTouchPoint(this).Position.X;
            imageDragStart = storyboard.GetCurrentProgress(this).GetValueOrDefault(0) * this.Width;
        }

        private void OnClick(object sender, MouseEventArgs args)
        {
            storyboard.Pause(this);
            mouseDragStart = args.GetPosition(this).X;
            imageDragStart = storyboard.GetCurrentProgress(this).GetValueOrDefault(0) * this.Width;
        }

        private void RestartAnimation(object sender, MouseEventArgs args)
        {
            storyboard.Resume(this);
        }

        private void SeekToMouse(object sender, MouseEventArgs args)
        {
            if (MouseButtonState.Pressed != args.LeftButton) return;

            storyboard.Pause(this);

            var percent = (args.GetPosition(this).X - mouseDragStart + imageDragStart) / this.Width;
            var seekTo = percent * animation.Duration.TimeSpan.TotalSeconds;
            if (seekTo < 0) { seekTo = 0; }
            storyboard.SeekAlignedToLastTick(this, TimeSpan.FromSeconds(seekTo), TimeSeekOrigin.BeginTime);
        }

        private void OnKeyUp(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Q) Close();
        }
    }
}

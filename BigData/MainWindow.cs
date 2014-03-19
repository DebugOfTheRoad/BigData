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

            var publications = App.GetOCLCClient().FetchPublicationsFromRSS(App.PublicationListUri);
            var images = from pub in publications
                         select new Image { Source = pub.CoverImage };

            canvas = new ImageCollectionCanvas(images);
            this.Content = canvas;

            NameScope.SetNameScope(this, new NameScope());
            canvas.RenderTransform = new TranslateTransform();
            RegisterName("transform", canvas.RenderTransform);

            animation = new DoubleAnimation(0, 1440, new Duration(TimeSpan.FromSeconds(120)));
            Storyboard.SetTargetName(animation, "transform");
            Storyboard.SetTargetProperty(animation, new PropertyPath(TranslateTransform.XProperty));

            storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin(this, true);
        }

        void MainWindow_TouchMove(object sender, TouchEventArgs e)
        {
            storyboard.Pause(this);

            var percent = (e.GetTouchPoint(this).Position.X - mouseDragStart + imageDragStart) / 1440;
            var seekTo = percent * animation.Duration.TimeSpan.TotalSeconds;
            if (seekTo < 0) { seekTo = 0; }
            storyboard.SeekAlignedToLastTick(this, TimeSpan.FromSeconds(seekTo), TimeSeekOrigin.BeginTime);
        }

        void MainWindow_TouchDown(object sender, TouchEventArgs e)
        {
            storyboard.Pause(this);
            mouseDragStart = e.GetTouchPoint(this).Position.X;
            imageDragStart = storyboard.GetCurrentProgress(this).GetValueOrDefault(0) * 1440;
        }

        private void OnClick(object sender, MouseEventArgs args)
        {
            storyboard.Pause(this);
            mouseDragStart = args.GetPosition(this).X;
            imageDragStart = storyboard.GetCurrentProgress(this).GetValueOrDefault(0) * 1440;
        }

        private void RestartAnimation(object sender, MouseEventArgs args)
        {
            storyboard.Resume(this);
        }

        private void SeekToMouse(object sender, MouseEventArgs args)
        {
            if (MouseButtonState.Pressed != args.LeftButton) return;

            storyboard.Pause(this);

            var percent = (args.GetPosition(this).X - mouseDragStart + imageDragStart) / 1440;
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

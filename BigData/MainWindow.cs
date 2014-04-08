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
using System.Diagnostics;

namespace BigData
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Grid grid;
        private PublicationCanvas[] views;
        private int activeViewIndex;
        private Stopwatch tapTimer;

        const int MAX_TAP_TIME = 150; // ms

        private void InitializeComponent()
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Visibility = Visibility.Visible;
            Title = "Digital Publication Display";

            KeyUp += OnKeyUp;
            Loaded += PopulateDisplay;

            grid = new Grid();
            Content = grid;

            ColumnDefinition col = new ColumnDefinition();
            grid.ColumnDefinitions.Add(col);

            RowDefinition row1 = new RowDefinition();
            RowDefinition row2 = new RowDefinition();
            RowDefinition row3 = new RowDefinition();
            grid.RowDefinitions.Add(row1);
            grid.RowDefinitions.Add(row2);
            grid.RowDefinitions.Add(row3);

            tapTimer = new Stopwatch();

            MouseDown += (sender, e) =>
            {
                activeViewIndex = (int)(e.GetPosition(this).Y * 3 / this.Height);
                views[activeViewIndex].BeginTouchTracking(e.GetPosition(this).X);
                tapTimer.Restart();
            };
            TouchDown += (sender, e) =>
            {
                activeViewIndex = (int)(e.GetTouchPoint(this).Position.Y % (this.Height / 3));
                views[activeViewIndex].BeginTouchTracking(e.GetTouchPoint(this).Position.X);
                tapTimer.Restart();
            };

            MouseUp += (sender, e) =>
            {
                views[activeViewIndex].EndTouchTracking(e.GetPosition(this).X);
                tapTimer.Stop();

                if (tapTimer.ElapsedMilliseconds < MAX_TAP_TIME)
                {
                    ShowPublicationAtPoint(e.GetPosition(this));
                }
            };
            TouchUp += (sender, e) =>
            {
                views[activeViewIndex].EndTouchTracking(e.GetTouchPoint(this).Position.X);
                tapTimer.Stop();

                if (tapTimer.ElapsedMilliseconds < MAX_TAP_TIME)
                {
                    ShowPublicationAtPoint(e.GetTouchPoint(this).Position);
                }
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
            var src = new OCLC.Database(Properties.Settings.Default.WSKey, Properties.Settings.Default.RSSUri);
            await src.createDatabase();
            var publications = (await src.GetPublications()).ToArray();

            var imagesPerRow = publications.Length / 3;

            views = new PublicationCanvas[3];

            views[0] = new PublicationCanvas(publications.Take(imagesPerRow).ToArray(), Height / 3);
            views[1] = new PublicationCanvas(publications.Skip(imagesPerRow).Take(imagesPerRow).ToArray(), Height / 3);
            views[2] = new PublicationCanvas(publications.Skip(imagesPerRow * 2).Take(imagesPerRow).ToArray(), Height / 3);

            for (int i = 0; i < views.Length; i++)
            {
                Grid.SetRow(views[i], i);
                grid.Children.Add(views[i]);
            }
        }

        void ShowPublicationAtPoint(Point point)
        {
            int index = (int)(point.Y * 3 / this.Height);
            var pub = views[index].GetPublicationAtPoint(point.X);

            var view = new InfoGrid(pub);
            Grid.SetRow(view, 0);
            Grid.SetRowSpan(view, 3);
            grid.Children.Add(view);

            view.Done += (s, e) =>
            {
                grid.Children.Remove(view);
            };
        }

        private void OnKeyUp(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Q) Close();
        }
    }
}

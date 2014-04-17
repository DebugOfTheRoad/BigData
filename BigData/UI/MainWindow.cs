using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
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
using System.Windows.Interop;

namespace BigData.UI {
    public class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private Grid grid;
        private PublicationCanvas[] views;
        private int activeViewIndex;
        private Stopwatch tapTimer;

        const int MAX_TAP_TIME = 75; // ms

        private void InitializeComponent() {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Visibility = Visibility.Visible;
            Title = "Digital Publication Display";

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
        }

        async void PopulateDisplay(object sender, RoutedEventArgs args) {
            FlashMessage("Loading...", Brushes.LightYellow);
            var src = new OCLC.Database(Properties.Settings.Default.WSKey, Properties.Settings.Default.RSSUri);
            await src.createDatabase();
            //var src = new OCLC.Client(Properties.Settings.Default.WSKey, Properties.Settings.Default.RSSUri);
            var publications = (await src.GetPublications()).ToArray();
            FlashMessage("Loaded", Brushes.LightGreen);

            var imagesPerRow = publications.Length / 3;

            views = new PublicationCanvas[3];

            views[0] = new PublicationCanvas(publications.Take(imagesPerRow).ToArray(), Height / 3);
            views[1] = new PublicationCanvas(publications.Skip(imagesPerRow).Take(imagesPerRow).ToArray(), Height / 3);
            views[2] = new PublicationCanvas(publications.Skip(imagesPerRow * 2).Take(imagesPerRow).ToArray(), Height / 3);

            for (int i = 0; i < views.Length; i++) {
                Grid.SetRow(views[i], i);
                grid.Children.Add(views[i]);
            }
            ///*
            //MouseDown += (_, e) => {
            //    activeViewIndex = (int)(e.GetPosition(this).Y * 3 / this.Height);
            //    views[activeViewIndex].BeginTouchTracking(e.GetPosition(this).X);
            //    tapTimer.Restart();
            //};
            // */ 
            //TouchDown += (_, e) => {
            //    activeViewIndex = (int)(e.GetTouchPoint(this).Position.Y * 3 / this.Height);
            //    views[activeViewIndex].BeginTouchTracking(e.GetTouchPoint(this).Position.X);
            //    tapTimer.Restart();
            //};

            ///*
            //MouseUp += (_, e) => {
            //    views[activeViewIndex].EndTouchTracking(e.GetPosition(this).X);
            //    tapTimer.Stop();

            //    if (tapTimer.ElapsedMilliseconds < MAX_TAP_TIME) {
            //        ShowPublicationAtPoint(e.GetPosition(this));
            //    }
            //};
            // */
            //TouchUp += (_, e) => {
            //    views[activeViewIndex].EndTouchTracking(e.GetTouchPoint(this).Position.X);
            //    tapTimer.Stop();

            //    if (tapTimer.ElapsedMilliseconds < MAX_TAP_TIME) {
            //        ShowPublicationAtPoint(e.GetTouchPoint(this).Position);
            //    }
            //};

            ///*
            //MouseMove += (_, e) => {
            //    views[activeViewIndex].TrackTouch(e.GetPosition(this).X);
            //};
            // */ 
            //TouchMove += (_, e) => {
            //    views[activeViewIndex].TrackTouch(e.GetTouchPoint(this).Position.X);
            //};

            StylusSystemGesture += (s, e) => {
                if (e.SystemGesture == SystemGesture.Tap) {
                    ShowPublicationAtPoint(e.GetPosition(this));
                }
            };

            MouseUp += (s, e) => {
                ShowPublicationAtPoint(e.GetPosition(this));
            };
        }

        /*
        void FlashMessage(string text, Brush background) {
            var label = new Label {
                Content = text,
                Background = background,
                FontFamily = new FontFamily("Segoe UI Light"),
                FontSize = 30,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                Height = 60,
            };
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, 0);
            Grid.SetZIndex(label, 10); 
            grid.Children.Add(label);

            var animation = new DoubleAnimation {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(5)),
            };
            label.ApplyAnimationClock(Label.OpacityProperty, animation.CreateClock());
            animation.Completed += delegate { grid.Children.Remove(label); };
        }
         */

        void FlashMessage(string text, Brush background) {
            var label = new Label {
                Content = text,
                Background = background,
                FontFamily = new FontFamily("Segoe UI Light"),
                FontSize = 30,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                Height = 60,
                RenderTransform = new TranslateTransform(0, -60),
            };
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, 0);
            Grid.SetZIndex(label, 10);
            grid.Children.Add(label);

            var inAnimation = new DoubleAnimation {
                From = -60,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
            };
            label.RenderTransform.ApplyAnimationClock(
                TranslateTransform.YProperty,
                inAnimation.CreateClock()
            );

            var timer = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(5),
            };
            timer.Start();
            timer.Tick += delegate {
                timer.Stop();
                var outAnimation = new DoubleAnimation {
                    From = 0,
                    To = -60,
                    Duration = new Duration(TimeSpan.FromSeconds(0.25)),
                    EasingFunction = new CubicEase {  EasingMode = EasingMode.EaseInOut },
                };
                label.RenderTransform.ApplyAnimationClock(
                    TranslateTransform.YProperty,
                    outAnimation.CreateClock()
                );
                outAnimation.Completed += delegate { grid.Children.Remove(label); };
            };

        }

        void ShowPublicationAtPoint(Point point) {
            int index = (int)(point.Y * 3 / this.Height);
            var pub = views[index].GetPublicationAtPoint(point.X);

            var view = new InfoGrid(pub);
            Grid.SetRow(view, 0);
            Grid.SetRowSpan(view, 3);
            grid.Children.Add(view);

            view.EmailSent += delegate {
                FlashMessage("Email Sent!", Brushes.LightGreen);
            };

            view.Done += (s, e) => {
                grid.Children.Remove(view);
            };
        }
    }
}

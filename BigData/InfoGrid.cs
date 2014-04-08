using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Input;

namespace BigData
{
    public class InfoGrid : Grid
    {
        const double EASE_IN_TIME = 0.1; // seconds
        public InfoGrid(Publication pub)
        {
            publication = pub;
            InitializeComponent();
        }

        public static readonly RoutedEvent DoneEvent = EventManager.RegisterRoutedEvent(
            "Done", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InfoGrid)
        );

        public event RoutedEventHandler Done
        {
            add { AddHandler(DoneEvent, value); }
            remove { RemoveHandler(DoneEvent, value); }
        }

        private Publication publication;

        void InitializeComponent()
        {
            Opacity = 0;
            Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0));

            ColumnDefinitions.Add(new ColumnDefinition());
            ColumnDefinitions.Add(new ColumnDefinition());
            RowDefinitions.Add(new RowDefinition());
            RowDefinitions.Add(new RowDefinition());
            RowDefinitions.Add(new RowDefinition());

            var image = new Image() { Source = publication.CoverImage };
            Grid.SetRow(image, 0);
            Grid.SetRowSpan(image, RowDefinitions.Count);
            Grid.SetColumn(image, 0);
            Children.Add(image);

            // capture all mouse events
            MouseUp += (sender, e) => { e.Handled = true; AnimateOut(sender, e); };
            MouseDown += (sender, e) => { e.Handled = true; };

            Loaded += AnimateIn;
        }

        void AnimateIn(object sender, RoutedEventArgs e)
        {
            var animation = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(EASE_IN_TIME)));
            var clock = animation.CreateClock();

            ApplyAnimationClock(Grid.OpacityProperty, clock);
        }

        void AnimateOut(object sender, EventArgs e)
        {
            var animation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(EASE_IN_TIME)));
            var clock = animation.CreateClock();

            ApplyAnimationClock(Grid.OpacityProperty, clock);
            clock.Completed += (s, e2) =>
            {
                var args = new RoutedEventArgs(InfoGrid.DoneEvent);
                RaiseEvent(args);
            };
        }
    }
}

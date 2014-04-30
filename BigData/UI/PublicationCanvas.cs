using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Input;

namespace BigData.UI {
    public class PublicationCanvas : Canvas {
        const double RESTING_VELOCITY = 0.05; // pixels per frame
        const double RENDER_TRANSFORM = -500; // offset render 500 pixels left
        const double DECELERATION = (50.0 * 96) / (1000 * 1000);

        public PublicationCanvas(Publication[] pubs, double height) {
            publications = pubs.ToDictionary(
                p => new Image() { Source = p.CoverImage, Height = height },
                p => p
            );
            InitializeComponent();
        }

        public static readonly RoutedEvent PublicationSelectedEvent = EventManager.RegisterRoutedEvent(
            "PublicationSelected",
            RoutingStrategy.Bubble,
            typeof(PublicationSelectedHandler),
            typeof(PublicationCanvas));

        public event PublicationSelectedHandler PublicationSelected {
            add { AddHandler(PublicationSelectedEvent, value); }
            remove { RemoveHandler(PublicationSelectedEvent, value); }
        }

        private Dictionary<Image, Publication> publications;
        private double tileWidth;

        void InitializeComponent() {
            IsManipulationEnabled = true;

            RenderTransform = new TranslateTransform() { X = RENDER_TRANSFORM };

            tileWidth = publications.Keys
                .Sum(im => (im.Height / im.Source.Height) * im.Source.Width);

            double offset = 0;
            foreach (var image in publications.Keys) {
                Children.Add(image);
                Canvas.SetLeft(image, offset);
                image.StylusSystemGesture += ImageTapped;
                image.SnapsToDevicePixels = true;
                offset += (image.Height / image.Source.Height) * image.Source.Width;
            }

            ManipulationStarting += BeginManipulation;
            ManipulationDelta += HandleManipulation;
            ManipulationInertiaStarting += BeginInertia;
            ManipulationCompleted += EndManipulation;

            CompositionTarget.Rendering += ScrollIfNotTouched;
        }

        void BeginManipulation(object sender, ManipulationStartingEventArgs args) {
            if (args.Mode == ManipulationModes.TranslateX) {
                CompositionTarget.Rendering -= ScrollIfNotTouched;
            }
        }

        void HandleManipulation(object sender, ManipulationDeltaEventArgs args) {
            var delta = args.DeltaManipulation.Translation.X;
            ScrollImagesBy(delta);
        }

        void BeginInertia(object sender, ManipulationInertiaStartingEventArgs args) {
            args.TranslationBehavior.DesiredDeceleration = DECELERATION;
        }

        void EndManipulation(object sender, ManipulationCompletedEventArgs args) {
            CompositionTarget.Rendering += ScrollIfNotTouched;
        }

        void ScrollIfNotTouched(object sender, EventArgs args) {
            ScrollImagesBy(RESTING_VELOCITY);
        }

        void ScrollImagesBy(double delta) {
            foreach (var image in Children.OfType<Image>()) {
                var nextX = (Canvas.GetLeft(image) + delta) % tileWidth;
                if (nextX < 0) { nextX += tileWidth; }
                Canvas.SetLeft(image, nextX);
            }
        }

        void ImageTapped(object sender, StylusSystemGestureEventArgs args) {
            if (args.SystemGesture != SystemGesture.Tap) { return; }

            var publication = publications[(Image)sender];
            RaiseEvent(new PublicationSelectedArgs(
                PublicationCanvas.PublicationSelectedEvent, publication));
        }
    }

    public class PublicationSelectedArgs : RoutedEventArgs {
        public PublicationSelectedArgs(RoutedEvent e, Publication p) {
            RoutedEvent = e;
            Publication = p;
        }
        public Publication Publication;
    }

    public delegate void PublicationSelectedHandler(object sender, PublicationSelectedArgs args);
}

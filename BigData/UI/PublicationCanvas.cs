﻿using System;
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

    /// <summary>
    /// A Canvas object displaying a horizontally-scrollable collection of
    /// Publication cover images
    /// </summary>
    public class PublicationCanvas : Canvas {

        /// <summary>
        /// Create and initialize a new PublicationCanvas
        /// </summary>
        /// <param name="pubs">The publications to display on the screen</param>
        /// <param name="height">The height of the canvas</param>
        public PublicationCanvas(Publication[] pubs, double height) {

            // map Image objects to Publications
            publications = pubs.ToDictionary(
                p => new Image() { Source = p.CoverImage, Height = height },
                p => p
            );

            // shift entire display 500px left
            RenderTransform = new TranslateTransform() { X = RENDER_TRANSFORM };

            // initially layout images
            tileWidth = 0;
            foreach (var image in publications.Keys) {
                Children.Add(image);
                image.RenderTransform = new TranslateTransform(tileWidth, 0);
                image.StylusSystemGesture += ImageTapped;
                image.SnapsToDevicePixels = true;
                tileWidth += (image.Height / image.Source.Height) * image.Source.Width;
            }

            // allow multitouch manipulation
            IsManipulationEnabled = true;
            ManipulationStarting += BeginManipulation;
            ManipulationDelta += HandleManipulation;
            ManipulationInertiaStarting += BeginInertia;
            ManipulationCompleted += EndManipulation;

            // register UI scroll at 60fps
            timer = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(1.0 / 60.0)
            };
            timer.Tick += delegate { ScrollImagesBy(RESTING_VELOCITY); };
            Loaded += delegate { timer.Start(); };
        }

        /// <summary>
        /// Raised when a publication is selected
        /// </summary>
        public event PublicationSelectedHandler PublicationSelected {
            add { AddHandler(PublicationSelectedEvent, value); }
            remove { RemoveHandler(PublicationSelectedEvent, value); }
        }

        Dictionary<Image, Publication> publications;
        double tileWidth;
        DispatcherTimer timer;

        void BeginManipulation(object sender, ManipulationStartingEventArgs args) {
            if (args.Mode == ManipulationModes.TranslateX) {
                timer.IsEnabled = false;
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
            timer.IsEnabled = true;
        }

        void ScrollImagesBy(double delta) {
            foreach (var image in publications.Keys) {
                var translation = (TranslateTransform)image.RenderTransform;

                var nextX = (translation.X + delta) % tileWidth;
                if (nextX < 0) { nextX += tileWidth; }

                translation.X = nextX;
            }
        }

        void ImageTapped(object sender, StylusSystemGestureEventArgs args) {
            if (args.SystemGesture != SystemGesture.Tap) { return; }

            var publication = publications[(Image)sender];
            RaiseEvent(new PublicationSelectedArgs(
                PublicationCanvas.PublicationSelectedEvent, publication));
        }

        const double RESTING_VELOCITY = 0.05; // pixels per frame
        const double RENDER_TRANSFORM = -500; // offset render 500 pixels left
        const double DECELERATION = (50.0 * 96) / (1000 * 1000);

        static readonly RoutedEvent PublicationSelectedEvent = EventManager.RegisterRoutedEvent(
            "PublicationSelected",
            RoutingStrategy.Bubble,
            typeof(PublicationSelectedHandler),
            typeof(PublicationCanvas));
    }

    /// <summary>
    /// EventArgs containing the Publication selected by the user
    /// </summary>
    public class PublicationSelectedArgs : RoutedEventArgs {
        public PublicationSelectedArgs(RoutedEvent e, Publication p) {
            RoutedEvent = e;
            Publication = p;
        }
        public Publication Publication;
    }

    /// <summary>
    /// Delegate describing a handler for the PublicationSelected event sent by
    /// PublicationCanvas objects.
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="args">A PublicationSelectedArgs object containing
    /// the selected Publication</param>
    public delegate void PublicationSelectedHandler(object sender, PublicationSelectedArgs args);
}

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
        const double RESTING_VELOCITY = 0.1; // pixels per frame
        const double DECELERATE_COEF = 0.9;
        const double MAX_STILL_TIME = 5000; // milliseconds
        const double MOUSE_WAIT_TIME = 16; // milliseconds
        const double RENDER_TRANSFORM = -500; // offset render 500 pixels left

        public PublicationCanvas(Publication[] pubs, double height) {
            publications = pubs.ToDictionary(
                p => new Image() { Source = p.CoverImage, Height = height },
                p => p
            );
            InitializeComponent();
        }

        public Publication GetPublicationAtPoint(double x) {
            x -= RENDER_TRANSFORM;
            foreach (var image in Children.OfType<Image>()) {
                var left = Canvas.GetLeft(image);
                var right = left + image.ActualWidth;

                if (left < x && right > x) {
                    return publications[image];
                }
            }

            throw new Exception("No publication at given point");
        }

        private bool isBeingManipulated;
        private Dictionary<Image, Publication> publications;
        private double tileWidth;

        void InitializeComponent() {
            IsManipulationEnabled = true;
           
            //CompositionTarget.Rendering += ScrollImages;
            //CompositionTarget.Rendering += UpdateVelocity;

            RenderTransform = new TranslateTransform() { X = RENDER_TRANSFORM };

            tileWidth = publications.Keys.Aggregate(
                0.0,
                (acc, im) => acc + (im.Height / im.Source.Height) * im.Source.Width
            );

            double offset = 0;
            foreach (var image in publications.Keys) {
                Children.Add(image);
                Canvas.SetLeft(image, offset);
                offset += (image.Height / image.Source.Height) * image.Source.Width;
            }

            ManipulationStarting += (s, e) => {
                if (e.Mode == ManipulationModes.TranslateX) {
                    isBeingManipulated = true;
                }
            };

            ManipulationDelta += (s, e) => {
                var delta = e.DeltaManipulation.Translation.X;
                ScrollImagesBy(delta);
            };

            ManipulationInertiaStarting += (s, e) => {
                // Deceleration is 50in/s^2
                e.TranslationBehavior.DesiredDeceleration = (50.0 * 96) / (1000 * 1000);
            };

            ManipulationCompleted += (s, e) => {
                isBeingManipulated = false;
            };

            CompositionTarget.Rendering += delegate {
                if (!isBeingManipulated) { ScrollImagesBy(0.1); };
            };
        }

        void ScrollImagesBy(double delta) {
            if (isBeingManipulated) { return; }

            foreach (var image in Children.OfType<Image>()) {
                var nextX = (Canvas.GetLeft(image) + delta) % tileWidth;
                if (nextX < 0) { nextX += tileWidth; }
                Canvas.SetLeft(image, nextX);
            }
        }
    }
}

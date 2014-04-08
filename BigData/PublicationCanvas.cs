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

namespace BigData
{
    public class PublicationCanvas : Canvas
    {
        const double RESTING_VELOCITY = 0.1; // pixels per frame
        const double DECELERATE_COEF = 0.9;
        const double MAX_STILL_TIME = 5000; // milliseconds
        const double MOUSE_WAIT_TIME = 16; // milliseconds
        const double RENDER_TRANSFORM = -500; // offset render 500 pixels left

        public PublicationCanvas(Publication[] pubs, double height)
        {
            publications = pubs.ToDictionary(
                p => new Image() { Source = p.CoverImage, Height = height },
                p => p
            );
            InitializeComponent();
        }

        public Publication GetPublicationAtPoint(double x)
        {
            x -= RENDER_TRANSFORM;
            foreach (var image in Children.OfType<Image>())
            {
                var left = Canvas.GetLeft(image);
                var right = left + image.ActualWidth;

                if (left < x && right > x)
                {
                    return publications[image];
                }
            }

            throw new Exception("No publication at given point");
        }

        public void BeginTouchTracking(double position)
        {
            isMouseDown = true;
            dragBegin = position;
            lastMousePosition = position;
            startingPositions = Children.OfType<Image>().ToDictionary(
                im => im,
                im => Canvas.GetLeft(im)
            );
        }

        public void EndTouchTracking(double position)
        {
            if (isMouseDown)
            {
                isMouseDown = false;
                scrollVelocity = position - lastMousePosition;
            }

            var timer = new Timer(MAX_STILL_TIME);
            timer.Elapsed += (s, a) =>
            {
                if (scrollVelocity == 0) { scrollVelocity = RESTING_VELOCITY; }
                timer.Stop();
            };
            timer.Start();
        }

        public void TrackTouch(double position)
        {
            if (!isMouseDown) { return; }

            double translation = position - dragBegin;
            foreach (var pair in startingPositions)
            {
                var image = pair.Key;
                var startingPosition = pair.Value;

                var nextX = (startingPosition + translation) % tileWidth;
                if (nextX < 0) { nextX += tileWidth; }
                Canvas.SetLeft(image, nextX);
            }

            var timer = new Timer(MOUSE_WAIT_TIME);
            timer.Elapsed += (s, a) =>
            {
                lastMousePosition = position;
                timer.Stop();
            };
            timer.Start();
        }

        private bool isMouseDown;
        private Dictionary<Image, Publication> publications;
        private Dictionary<Image, double> startingPositions;
        private double dragBegin;
        private double scrollVelocity;
        private double lastMousePosition;
        private double tileWidth;

        void InitializeComponent()
        {
            CompositionTarget.Rendering += ScrollImages;
            CompositionTarget.Rendering += UpdateVelocity;

            RenderTransform = new TranslateTransform() { X = RENDER_TRANSFORM };
            scrollVelocity = RESTING_VELOCITY;

            tileWidth = publications.Keys.Aggregate(
                0.0,
                (acc, im) => acc + (im.Height / im.Source.Height) * im.Source.Width
            );

            double offset = 0;
            foreach (var image in publications.Keys)
            {
                Children.Add(image);
                Canvas.SetLeft(image, offset);
                offset += (image.Height / image.Source.Height) * image.Source.Width;
            }
        }

        void ScrollImages(object sender, EventArgs e)
        {
            if (isMouseDown) { return; }

            foreach (var image in Children.OfType<Image>())
            {
                var nextX = (Canvas.GetLeft(image) + scrollVelocity) % tileWidth;
                if (nextX < 0) { nextX += tileWidth; }
                Canvas.SetLeft(image, nextX);
            }
        }

        void UpdateVelocity(object sender, EventArgs e)
        {
            if (Math.Abs(scrollVelocity) > RESTING_VELOCITY)
            {
                scrollVelocity *= DECELERATE_COEF;
            }
            else
            {
                scrollVelocity = RESTING_VELOCITY * Math.Sign(scrollVelocity);
            }
        }
    }
}

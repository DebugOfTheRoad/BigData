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

namespace BigData
{
    public class WrappingCollectionView : Canvas
    {
        public WrappingCollectionView(Image[] images)
        {
            this.images = images;
            InitializeComponent();
        }

        private Image[] images;
        private bool isMouseDown;
        private Dictionary<Image, double> startingPositions;
        private double dragBegin;
        private double scrollVelocity;
        private double lastMousePosition;
        private double tileWidth;

        void InitializeComponent()
        {
            CompositionTarget.Rendering += ScrollImages;
            CompositionTarget.Rendering += UpdateVelocity;

            MouseDown += (sender, e) => { BeginTouchTracking(e.GetPosition(this).X); };
            TouchDown += (sender, e) => { BeginTouchTracking(e.GetTouchPoint(this).Position.X); };

            MouseUp += (sender, e) => { EndTouchTracking(e.GetPosition(this).X); };
            TouchUp += (sender, e) => { EndTouchTracking(e.GetTouchPoint(this).Position.X); };

            MouseMove += (sender, e) => { TrackTouch(e.GetPosition(this).X); };
            TouchMove += (sender, e) => { TrackTouch(e.GetTouchPoint(this).Position.X); };


            RenderTransform = new TranslateTransform() { X = -500 };
            scrollVelocity = 0.5;

            tileWidth = images.Aggregate(
                0.0,
                (acc, im) => acc + (im.Height / im.Source.Height) * im.Source.Width
            );

            double offset = 0;
            foreach (var image in images)
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
            if (Math.Abs(scrollVelocity) > 0.5)
            {
                scrollVelocity *= 0.9;
            }
            else
            {
                scrollVelocity = 0.5 * Math.Sign(scrollVelocity);
            }
        }

        void BeginTouchTracking(double position)
        {
            isMouseDown = true;
            dragBegin = position;
            lastMousePosition = position;
            startingPositions = Children.OfType<Image>().ToDictionary(
                im => im,
                im => Canvas.GetLeft(im)
            );
        }

        void EndTouchTracking(double position)
        {
            isMouseDown = false;
            scrollVelocity = position - lastMousePosition;
        }

        void TrackTouch(double position)
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

            var timer = new Timer(16);
            timer.Elapsed += (s, a) =>
            {
                lastMousePosition = position;
                timer.Stop();
            };
            timer.Start();
        }
    }
}

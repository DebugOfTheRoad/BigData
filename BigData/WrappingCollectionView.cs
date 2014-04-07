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

namespace BigData
{
    public class WrappingCollectionView : Image
    {
        public WrappingCollectionView(ImageSource[] sources)
        {
            imageSources = sources;
            InitializeComponent();
            Loaded += BeginAnimation;
        }

        private ImageSource[] imageSources;
        private ImageDrawing[] images;
        private DrawingGroup drawingGroup;

        void InitializeComponent()
        {
            drawingGroup = new DrawingGroup();
            images = new ImageDrawing[imageSources.Length];

            double offset = 0;
            for (int i = 0; i < imageSources.Length; i++)
            {
                if (!imageSources[i].IsFrozen)
                {
                    imageSources[i].Freeze();
                }

                double height = 300;
                var width = (height / imageSources[i].Height) * imageSources[i].Width;

                images[i] = new ImageDrawing()
                {
                    Rect = new Rect(offset, 0, width, height),
                    ImageSource = imageSources[i]
                };
                drawingGroup.Children.Add(images[i]);
                offset += images[i].Rect.Width;
            }

            Source = new DrawingImage(drawingGroup);

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            images[0].Rect.Offset(1, 0); 
        }

        void BeginAnimation(object sender, RoutedEventArgs e)
        {
            var animation = new DoubleAnimation(100, new Duration(TimeSpan.FromSeconds(5)));
            animation.AutoReverse = true;

            var transform = new TranslateTransform();
            drawingGroup.Transform = transform;

            var clock = animation.CreateClock();
            transform.ApplyAnimationClock(TranslateTransform.YProperty, clock, HandoffBehavior.SnapshotAndReplace);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            
        }
    }
}

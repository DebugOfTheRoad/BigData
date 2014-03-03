using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BigData
{
    class ImageCollectionCanvas : Canvas
    {
        public ImageCollectionCanvas(Image[] images)
        {
            this.LayoutUpdated += LayoutImages;

            foreach (Image im in images)
            {
                this.Children.Add(im);
            }
        }

        private void LayoutImages(object sender, EventArgs e)
        {
            double horizontalOffset = -1000;
            double verticalOffset = 0;
            foreach (var child in this.Children)
            {
                if (!(child is Image))
                {
                    throw new Exception("ImageCollectionCanvas should not have a child of type " + child.GetType().ToString());
                }

                var image = (Image)child;
                image.Height = this.ActualHeight / 3;

                SetTop(image, verticalOffset);
                SetLeft(image, horizontalOffset);

                horizontalOffset += image.ActualWidth;

                if (horizontalOffset > 1000)
                {
                    horizontalOffset = -1000;
                    verticalOffset += image.ActualHeight;
                }
            }
        }

        private Image[] images;

        public static Image CatcherInTheRye()
        {
            var image = new Image();
            image.Source = new BitmapImage(
                new Uri(@"pack://application:,,,/Resources/catcher.jpg", UriKind.RelativeOrAbsolute)
                );
            return image;
        }
    }
}

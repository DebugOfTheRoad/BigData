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
        public ImageCollectionCanvas()
        {
            this.LayoutUpdated += LayoutImages;
        }

        public Image[] Images { get; set; }
        public double TileWidth { get; set; }

        private void LayoutImages(object sender, EventArgs e)
        {
            double horizontalOffset = -TileWidth/2;
            double verticalOffset = 0;
            foreach (var image in Images)
            {
                if (!Children.Contains(image)) Children.Add(image);

                image.Height = 300;
                SetTop(image, verticalOffset);
                SetLeft(image, horizontalOffset);

                horizontalOffset += image.ActualWidth;
            }
        }
        
    }
}

﻿using System;
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
        public ImageCollectionCanvas(IEnumerable<Image> images)
        {
            this.images = images.ToArray();
            this.LayoutUpdated += LayoutImages;
        }

        private void LayoutImages(object sender, EventArgs e)
        {
            double horizontalOffset = -1000;
            double verticalOffset = 0;
            foreach (var image in images)
            {
                if (!Children.Contains(image)) Children.Add(image);
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
    }
}

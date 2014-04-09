using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Diagnostics;

namespace BigData.UI {
    class DetailLabel : Label {

        public DetailLabel() {
            InitializeComponent();
        }

        void InitializeComponent() {
            Background = Brushes.White.Clone();
            Background.Opacity = 0;

            Foreground = Brushes.White;
            FontSize = 40;
            FontFamily = new FontFamily("Segoe UI Light");
            Margin = new Thickness(50);

            MouseUp += AnimateTap;
            TouchUp += AnimateTap;
        }

        void AnimateTap(object sender, EventArgs args) {
            var animation = new DoubleAnimation {
                From = 0.5,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.1))
            };
            Background.ApplyAnimationClock(
                Brush.OpacityProperty,
                animation.CreateClock()
            );

            Process.Start(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe");
        }
    }
}

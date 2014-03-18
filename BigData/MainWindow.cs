using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BigData
{
    public class MainWindow : Window
    {
        private Canvas canvas;
        private Image image;
        private Storyboard storyboard;
        private DoubleAnimation animation;
        private double mouseDragStart;
        private double imageDragStart;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;
            this.Visibility = System.Windows.Visibility.Visible;
            this.Title = "Digital Publication Display";
            this.KeyUp += this.OnKeyUp;
            this.MouseMove += this.SeekToMouse;
            this.MouseUp += this.RestartAnimation;
            this.MouseDown += this.OnClick;

            var publications = App.GetOCLCClient().FetchPublicationsFromRSS(App.PublicationListUri);
            var images = from pub in publications
                         select new Image { Source = pub.CoverImage };

            canvas = new ImageCollectionCanvas(images);
            this.Content = canvas;

            //image = new Image();
            //image.Source = RenderArtwork();
            //canvas.Children.Add(image);

            NameScope.SetNameScope(this, new NameScope());
            canvas.RenderTransform = new TranslateTransform();
            RegisterName("transform", canvas.RenderTransform);

            animation = new DoubleAnimation(0, 1440, new Duration(TimeSpan.FromSeconds(60)));
            Storyboard.SetTargetName(animation, "transform");
            Storyboard.SetTargetProperty(animation, new PropertyPath(TranslateTransform.XProperty));

            storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin(this, true);
        }

        /*
        private ushort[] GlyphIndicesForString(string str, GlyphTypeface typeface)
        {
            return str.ToCharArray()
                .Select(c => typeface.CharacterToGlyphMap[c])
                .ToArray();
        }

        private double[] AdvanceWidthsForString(string str, GlyphTypeface typeface)
        {
            return str.ToCharArray()
                .Select(c => typeface.CharacterToGlyphMap[c])
                .Select(g => typeface.AdvanceWidths[g] * 16.0)
                .ToArray();
        }

        private ImageSource RenderArtwork()
        {
            OCLCWrapper wrapper = new OCLCWrapper(App.PublicationListUri, App.WSKey, App.SecretKey);
            List<Publication> list = wrapper.createPublications();

            var typeface = new GlyphTypeface(new Uri(@"file://C:\WINDOWS\fonts\segoeui.ttf"));
            
            var group = new DrawingGroup();
            var catcher = new BitmapImage(
                new Uri(@"pack://application:,,,/Resources/catcher.jpg", UriKind.RelativeOrAbsolute)
            );

            double imageHeight = 300;
            double imageWidth = (imageHeight / catcher.PixelHeight) * catcher.PixelWidth;

            for (var i = 0; i < list.Count; i++)
            {
                var drawing = new GlyphRunDrawing(Brushes.CornflowerBlue, new GlyphRun(
                    typeface,
                    0,
                    false,
                    16.0,
                    GlyphIndicesForString(list[i].oclcNumber, typeface),
                    new Point(i * imageWidth, 0),
                    AdvanceWidthsForString(list[i].oclcNumber, typeface),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null
                    ));
                group.Children.Add(drawing);
                Console.WriteLine(drawing.Bounds.Height);

                /*
                group.Children.Add(new ImageDrawing(
                    catcher,
                    new Rect(i * imageWidth, 0, imageWidth, imageHeight)
                ));
                group.Children.Add(new ImageDrawing(
                    catcher,
                    new Rect(i * imageWidth, imageHeight, imageWidth, imageHeight)
                ));
                group.Children.Add(new ImageDrawing(
                    catcher,
                    new Rect(i * imageWidth, imageHeight * 2, imageWidth, imageHeight)
                ));
                 
            }

            var source = new DrawingImage(group);
            source.Freeze();
            return source;
        }
        */

        private void OnClick(object sender, MouseEventArgs args)
        {
            storyboard.Pause(this);
            mouseDragStart = args.GetPosition(this).X;
            imageDragStart = storyboard.GetCurrentProgress(this).GetValueOrDefault(0) * 1440;
        }

        private void RestartAnimation(object sender, MouseEventArgs args)
        {
            storyboard.Resume(this);
        }

        private void SeekToMouse(object sender, MouseEventArgs args)
        {
            if (MouseButtonState.Pressed != args.LeftButton) return;

            storyboard.Pause(this);

            var percent = (args.GetPosition(this).X - mouseDragStart + imageDragStart) / 1440;
            var seekTo = percent * animation.Duration.TimeSpan.TotalSeconds;
            if (seekTo < 0) { seekTo = 0; }
            storyboard.SeekAlignedToLastTick(this, TimeSpan.FromSeconds(seekTo), TimeSeekOrigin.BeginTime);
        }

        private void OnKeyUp(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Q) Close();
        }
    }
}

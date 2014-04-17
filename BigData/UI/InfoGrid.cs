﻿using System;
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
using System.Windows.Input;
using System.Windows.Documents;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BigData.UI {
    public class InfoGrid : Grid {

        const double EASE_IN_TIME = 0.1; // seconds
        Process keyboardProcess;

        public InfoGrid(Publication pub) {
            publication = pub;
            InitializeComponent();
        }

        public static readonly RoutedEvent DoneEvent = EventManager.RegisterRoutedEvent(
            "Done", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InfoGrid)
        );

        public event RoutedEventHandler Done {
            add { AddHandler(DoneEvent, value); }
            remove { RemoveHandler(DoneEvent, value); }
        }

        public static readonly RoutedEvent EmailSentEvent = EventManager.RegisterRoutedEvent(
            "EmailSent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InfoGrid)
        );

        public event RoutedEventHandler EmailSent {
            add { AddHandler(EmailSentEvent, value); }
            remove { RemoveHandler(EmailSentEvent, value); }
        }

        private Publication publication;
        private bool showingTextBox;
        private TextBox box;

        void InitializeComponent() {
            showingTextBox = false;

            Opacity = 0;
            Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0));

            ColumnDefinitions.Add(new ColumnDefinition() {
                Width = new GridLength(4, GridUnitType.Star)
            });
            ColumnDefinitions.Add(new ColumnDefinition() {
                Width = new GridLength(6, GridUnitType.Star)
            });

            var image = new Image() {
                Source = publication.CoverImage,
                Margin = new Thickness { Left = 0, Right = 0, Top = 200, Bottom = 200 },
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(image, 0);
            Children.Add(image);

            var panel = new StackPanel {
                Orientation = Orientation.Vertical,
            };
            Grid.SetColumn(panel, 1);
            Children.Add(panel);

            var title = new TextBlock {
                Text = publication.Title,
                Foreground = Brushes.White,
                FontSize = 50,
                FontFamily = new FontFamily("Segoe UI Light"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(50, 200, 50, 0),
            };
            panel.Children.Add(title);

            var author = new TextBlock {
                Text = publication.Authors.DefaultIfEmpty("").First(),
                Foreground = Brushes.White,
                FontSize = 40,
                FontFamily = new FontFamily("Segoe UI Light"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(50, 0, 50, 0),
            };
            panel.Children.Add(author);

            var label = new TextBlock {
                Text = "Borrow Now ›",
                Foreground = Brushes.White,
                FontSize = 40,
                FontFamily = new FontFamily("Segoe UI Light"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(50, 50, 50, 0),
            };
            
            panel.Children.Add(label);

            box = new TextBox {
                Margin = new Thickness(50, 0, 50, 0),
                FontSize = 36,
                Text = "Username",
                RenderTransform = new TranslateTransform(0, 500),
                Width = 400,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            };
            box.KeyUp += (sender, args) => {
                if (args.Key != Key.Enter) { return; }
                Emailer.Emailer.emailSend(box.Text, publication);
                AnimateOut();
                RaiseEvent(new RoutedEventArgs(InfoGrid.EmailSentEvent));
            };
            panel.Children.Add(box);

            label.StylusSystemGesture += (_, e) => {
                if (e.SystemGesture == SystemGesture.Tap) {
                    ShowTextBox();
                    e.Handled = true;
                }
            };
            label.MouseUp += (_, e) => {
                e.Handled = true;
                ShowTextBox();
            };

            StylusSystemGesture += (_, e) => {
                if (e.SystemGesture == SystemGesture.Tap) {
                    e.Handled = true;
                    AnimateOut();
                }
            };
            MouseUp += (_, e) => {
                e.Handled = true;
                AnimateOut();
            };

            Loaded += AnimateIn;
        }


        void ShowTextBox() {
            if (!showingTextBox) {
                var animation = new DoubleAnimation {
                    From = 500,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                };
                box.RenderTransform.ApplyAnimationClock(TranslateTransform.YProperty, animation.CreateClock());
                showingTextBox = true;
            }

            box.SelectAll();
            box.Focus();

            keyboardProcess = Process.Start(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe");
        }

        void AnimateIn(object sender, RoutedEventArgs e) {
            var animation = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(EASE_IN_TIME)));
            var clock = animation.CreateClock();

            ApplyAnimationClock(Grid.OpacityProperty, clock);
        }

        void AnimateOut() {
            var animation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(EASE_IN_TIME)));
            var clock = animation.CreateClock();

            ApplyAnimationClock(Grid.OpacityProperty, clock);
            clock.Completed += (s, e2) => {
                var args = new RoutedEventArgs(InfoGrid.DoneEvent);
                RaiseEvent(args);
            };

            var window = FindWindow("IPTip_Main_Window", null);
            const uint WM_SYSCOMMAND = 274;
            IntPtr SC_CLOSE = (IntPtr)61536;
            PostMessage(window, WM_SYSCOMMAND, SC_CLOSE, (IntPtr)0);
        }

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}

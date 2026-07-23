using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SignalViewerPlayground.Controls
{
    public partial class LoadingSpinner : UserControl
    {
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(LoadingSpinner),
                new PropertyMetadata(false, OnIsActiveChanged));


        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        new public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(LoadingSpinner),
                new PropertyMetadata(36.0, OnFontSizeChanged));


        new public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Color), typeof(LoadingSpinner),
                new PropertyMetadata(Colors.LightGray, OnColorChanged));


        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }


        public LoadingSpinner()
        {
            InitializeComponent();
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not LoadingSpinner ctrl) return;

            var storyboard = ctrl.Resources["SpinStoryboard"] as Storyboard;
            if (ctrl.IsActive)
            {
                ctrl.Visibility = Visibility.Visible;
                storyboard?.Begin();
            }
            else
            {
                ctrl.Visibility = Visibility.Collapsed;
                storyboard?.Begin();
            }
        }

        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not LoadingSpinner ctrl) return;

            ctrl.Spinner.FontSize = ctrl.FontSize;
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not LoadingSpinner ctrl) return;

            ctrl.Spinner.Foreground = new SolidColorBrush(ctrl.Color);
        }
    }
}
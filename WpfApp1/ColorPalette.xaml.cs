using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ColorPalette.xaml
    /// </summary>
    public partial class ColorPalette : Window
    {
        private Ellipse colorMarker;
        dynamic renderTargetBitmap;
        readonly Border ColorPreview;
        readonly Label TextPreview;
        public ColorPalette(Border ColorPreview, Label TextPreview)
        {
            InitializeComponent();
            this.ColorPreview = ColorPreview;
            this.TextPreview = TextPreview;
        }

        public LinearGradientBrush Draw()
        {
            GradientStop gradientStop1 = new(Color.FromRgb(255, 0, 0), 0);
            GradientStop gradientStop2 = new(Color.FromRgb(255, 0, 255), 0.167);
            GradientStop gradientStop3 = new(Color.FromRgb(0, 0, 255), 0.333);
            GradientStop gradientStop4 = new(Color.FromRgb(0, 255, 255), 0.5);
            GradientStop gradientStop5 = new(Color.FromRgb(0, 255, 0), 0.667);
            GradientStop gradientStop6 = new(Color.FromRgb(255, 255, 0), 0.833);
            GradientStop gradientStop7 = new(Color.FromRgb(255, 0, 0), 1);

            GradientStopCollection gradientStops = new();
            gradientStops.Add(gradientStop1);
            gradientStops.Add(gradientStop2);
            gradientStops.Add(gradientStop3);
            gradientStops.Add(gradientStop4);
            gradientStops.Add(gradientStop5);
            gradientStops.Add(gradientStop6);
            gradientStops.Add(gradientStop7);

            LinearGradientBrush gradient = new() { GradientStops = gradientStops };
            
            return gradient;
        }

        private int CheckCoord(int coord, int maxBounds)
        {
            int newCoord = 0;

            if (coord > maxBounds)
            {
                newCoord = maxBounds;
            }
            else if (coord! > 0)
            {
                newCoord = coord;
            }

            return newCoord;
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int x = (int)e.GetPosition(ColorRec).X;
            int y = (int)e.GetPosition(ColorRec).Y;
            if ((x < renderTargetBitmap.PixelWidth) && (x > 0) && (y < renderTargetBitmap.PixelHeight) && (y > 0))
            {
                CroppedBitmap croppedBitmap = new(renderTargetBitmap, new Int32Rect(x, y, 1, 1));
                byte[] pixels = new byte[4];
                croppedBitmap.CopyPixels(pixels, 4, 0);
                Canvas.SetLeft(colorMarker, x - (colorMarker.Width / 2));
                Canvas.SetTop(colorMarker, y - (colorMarker.Height / 2));
                colorMarker.Fill = new SolidColorBrush(Color.FromRgb(pixels[2], pixels[1], pixels[0]));
                ColorPreview.Background = new SolidColorBrush(Color.FromRgb(pixels[2], pixels[1], pixels[0]));


                if (ColorPreview.Name == "ThemeColorPreview")
                {
                    SettingsVariables.themeColor = $"#{pixels[2]:X2}{pixels[1]:X2}{pixels[0]:X2}";
                    TextPreview.Content = SettingsVariables.themeColor;
                    if (pixels[2] <= 235 && pixels[1] <= 235 && pixels[0] <= 235)
                    {
                        pixels[2] += 20;
                        pixels[1] += 20;
                        pixels[0] += 20;
                    }
                    SettingsVariables.themeColor2 = $"#{pixels[2]:X2}{pixels[1]:X2}{pixels[0]:X2}";
                    Functions.PaintUI("themeColor");
                }
                else if(ColorPreview.Name == "FontColorPreview")
                {
                    SettingsVariables.fontColor = $"#{pixels[2]:X2}{pixels[1]:X2}{pixels[0]:X2}";
                    TextPreview.Content = SettingsVariables.fontColor;
                    if (pixels[2] >= 50 && pixels[1] >= 50 && pixels[0] >= 50)
                    {
                        pixels[2] -= 50;
                        pixels[1] -= 50;
                        pixels[0] -= 50;
                    }
                    SettingsVariables.fontColor2 = $"#{pixels[2]:X2}{pixels[1]:X2}{pixels[0]:X2}";
                    Functions.PaintUI("fontColor");
                }

            }


        }

        private void ColorRec_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                int x = (int)e.GetPosition(ColorRec).X;
                int y = (int)e.GetPosition(ColorRec).Y;
                if ((x < renderTargetBitmap.PixelWidth) && (x > 0) && (y < renderTargetBitmap.PixelHeight) && (y > 0))
                {
                    CroppedBitmap croppedBitmap = new(renderTargetBitmap, new Int32Rect(x, y, 1, 1));
                    byte[] pixels = new byte[4];
                    croppedBitmap.CopyPixels(pixels, 4, 0);
                    Canvas.SetLeft(colorMarker, x - (colorMarker.Width / 2));
                    Canvas.SetTop(colorMarker, y - (colorMarker.Height / 2));
                    colorMarker.Fill = new SolidColorBrush(Color.FromRgb(pixels[2], pixels[1], pixels[0]));
                    ColorPreview.Background = new SolidColorBrush(Color.FromRgb(pixels[2], pixels[1], pixels[0]));
                    
                    if (ColorPreview.Name == "ThemeColorPreview")
                    {
                        SettingsVariables.themeColor = $"#{pixels[2]:X2}{pixels[1]:X2}{pixels[0]:X2}";
                        TextPreview.Content = SettingsVariables.themeColor;
                        if (pixels[2] <= 235 && pixels[1] <= 235 && pixels[0] <= 235)
                        {
                            pixels[2] += 20;
                            pixels[1] += 20;
                            pixels[0] += 20;
                        }
                        SettingsVariables.themeColor2 = $"#{pixels[2]:X2}{pixels[1]:X2}{pixels[0]:X2}";
                        Functions.PaintUI("themeColor");
                    }
                    else if (ColorPreview.Name == "FontColorPreview")
                    {
                        SettingsVariables.fontColor = $"#{pixels[2]:X2}{pixels[1]:X2}{pixels[0]:X2}";
                        TextPreview.Content = SettingsVariables.fontColor;
                        if (pixels[2] >= 50 && pixels[1] >= 50 && pixels[0] >= 50)
                        {
                            pixels[2] -= 50;
                            pixels[1] -= 50;
                            pixels[0] -= 50;
                        }
                        SettingsVariables.fontColor2 = $"#{pixels[2]:X2}{pixels[1]:X2}{pixels[0]:X2}";
                        Functions.PaintUI("fontColor");
                    }
                }
            }
        }

        private void HueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue <= 255)
            {
                Resources["CurrentColor"] = new Color() { A = 255, R = 255, G = 0, B = (byte)e.NewValue };
            }
            else if (e.NewValue <= 510)
            {
                Resources["CurrentColor"] = new Color() { A = 255, R = (byte)(255 - (e.NewValue - 255)), G = 0, B = 255 };
            }
            else if (e.NewValue <= 765)
            {
                Resources["CurrentColor"] = new Color() { A = 255, R = 0, G = (byte)(e.NewValue - 510), B = 255 };
            }
            else if (e.NewValue <= 1020)
            {
                Resources["CurrentColor"] = new Color() { A = 255, R = 0, G = 255, B = (byte)(255 - (e.NewValue - 765)) };
            }
            else if (e.NewValue <= 1275)
            {
                Resources["CurrentColor"] = new Color() { A = 255, R = (byte)(e.NewValue - 1020), G = 255, B = 0 };
            }
            else if (e.NewValue <= 1530)
            {
                Resources["CurrentColor"] = new Color() { A = 255, R = 255, G = (byte)(255 - (e.NewValue - 1275)), B = 0 };
            }

            int x = CheckCoord((int)Canvas.GetLeft(colorMarker), (int)ColorRec.ActualWidth);
            int y = CheckCoord((int)Canvas.GetTop(colorMarker), (int)ColorRec.ActualHeight);

            colorMarker.Visibility = Visibility.Hidden;
            renderTargetBitmap.Render(ColorRec);

            CroppedBitmap croppedBitmap = new(renderTargetBitmap, new Int32Rect(x, y, 1, 1));
            byte[] pixels = new byte[4];
            croppedBitmap.CopyPixels(pixels, 4, 0);
            colorMarker.Fill = new SolidColorBrush(Color.FromRgb(pixels[2], pixels[1], pixels[0]));
            ColorPreview.Background = new SolidColorBrush(Color.FromRgb(pixels[2], pixels[1], pixels[0]));

            if (ColorPreview.Name == "ThemeColorPreview")
            {
                SettingsVariables.themeColor = $"#{pixels[2]:X2}{pixels[1]:X2}{pixels[0]:X2}";
                TextPreview.Content = SettingsVariables.themeColor;
                if (pixels[2] <= 235 && pixels[1] <= 235 && pixels[0] <= 235)
                {
                    pixels[2] += 20;
                    pixels[1] += 20;
                    pixels[0] += 20;
                }
                SettingsVariables.themeColor2 = $"#{pixels[2]:X2}{pixels[1]:X2}{pixels[0]:X2}";
                Functions.PaintUI("themeColor");
            }
            else if (ColorPreview.Name == "FontColorPreview")
            {
                SettingsVariables.fontColor = $"#{pixels[2]:X2}{pixels[1]:X2}{pixels[0]:X2}";
                TextPreview.Content = SettingsVariables.fontColor;
                if (pixels[2] >= 50 && pixels[1] >= 50 && pixels[0] >= 50)
                {
                    pixels[2] -= 50;
                    pixels[1] -= 50;
                    pixels[0] -= 50;
                }
                SettingsVariables.fontColor2 = $"#{pixels[2]:X2}{pixels[1]:X2}{pixels[0]:X2}";
                Functions.PaintUI("fontColor");
            }

            colorMarker.Visibility = Visibility.Visible;
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ColorRec_Loaded(object sender, RoutedEventArgs e)
        {
            renderTargetBitmap = new RenderTargetBitmap((int)ColorRec.ActualWidth, (int)ColorRec.ActualHeight, 96, 96, PixelFormats.Default);
            renderTargetBitmap.Render(ColorRec);

            colorMarker = new();
            colorMarker.Width = 20;
            colorMarker.Height = 20;
            colorMarker.StrokeThickness = 2;
            colorMarker.Stroke = new SolidColorBrush(Colors.DarkSlateGray);

            ColorRec.Children.Add(colorMarker);
            Canvas.SetLeft(colorMarker, 0 - (colorMarker.Width / 2));
            Canvas.SetTop(colorMarker, 0 - (colorMarker.Height / 2));
        }
    }
}

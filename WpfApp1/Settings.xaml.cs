using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public List<StackPanel> rowPanels;
        public List<Image> images;

        private bool saved = false;
        public Settings(List<StackPanel> rowPanels, List<Image> images)
        {
            this.rowPanels = rowPanels;
            this.images = images;

            InitializeComponent();

            HeightSlider.Value = SettingsVariables.height;
            HeightLabel.Content = "Row height: " + SettingsVariables.height;
            HeightSlider.ValueChanged += Slider_ValueChanged;

            ThemeColorPreview.Background = new SolidColorBrush(Functions.SetColor(SettingsVariables.themeColor));
            FontColorPreview.Background = new SolidColorBrush(Functions.SetColor(SettingsVariables.fontColor));

            ThemeColorTextPreview.Content = SettingsVariables.themeColor;
            FontColorTextPreview.Content = SettingsVariables.fontColor;

        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            IsOpened.SettingsIsOpen = false;
            
            if (saved)
            {
                SettingsVariables.SaveSettings();
            }
            else
            {
                SettingsVariables.LoadSettings();
                Functions.PaintUI("themeColor");
                Functions.PaintUI("fontColor");
                for (int i = 0; i < rowPanels.Count; i++)
                {
                    rowPanels[i].Height = SettingsVariables.height;
                }
                for (int i = 0; i < images.Count; i++)
                {
                    images[i].Height = SettingsVariables.height;
                    images[i].Width = SettingsVariables.height;
                }
            }

            Close();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(SaveButton.IsEnabled == false)
            {
                SaveButton.IsEnabled = true;
                SaveButtonShadow.ShadowDepth = 5;
            }
            for(int i = 0; i < rowPanels.Count; i++)
            {
                rowPanels[i].Height = e.NewValue;
            }
            for (int i = 0; i < images.Count; i++)
            {
                images[i].Height = e.NewValue;
                images[i].Width = e.NewValue;
            }
            SettingsVariables.height = e.NewValue;
            HeightLabel.Content = "Row height: " + SettingsVariables.height;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            saved = true;
            SaveButtonShadow.ShadowDepth = 2;
            SaveButton.IsEnabled = false;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ColorPalette colorPalette = new(ThemeColorPreview, ThemeColorTextPreview);
            colorPalette.Show();
            saved = false;
        }

        private void Default(object sender, RoutedEventArgs e)
        {
            SettingsVariables.LoadDefault();
            Functions.PaintUI("themeColor");
            Functions.PaintUI("fontColor");
        }

        private void FontColorPicker(object sender, MouseButtonEventArgs e)
        {
            ColorPalette colorPalette = new(FontColorPreview, FontColorTextPreview);
            colorPalette.Show();
        }

    }
}

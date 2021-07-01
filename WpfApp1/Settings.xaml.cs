using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public List<RowDefinition> rowDefinitions;
        public List<Image> images;
        List<TextBlock> titleTexts;
        
        private bool saved = false;
        public Settings(List<RowDefinition> rowDefinitions, List<Image> images, List<TextBlock> titleTexts)
        {
            this.rowDefinitions = rowDefinitions;
            this.images = images;
            this.titleTexts = titleTexts;

            InitializeComponent();

            HeightSlider.Value = SettingsVariables.height;
            HeightLabel.Content = "Row height: " + SettingsVariables.height;
            HeightSlider.ValueChanged += Slider_ValueChanged;

            FontSizeSlider.Value = SettingsVariables.fontSize;
            FontSizeLabel.Content = "Row height: " + SettingsVariables.fontSize;
            FontSizeSlider.ValueChanged += FontSizeChanged;

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
            for(int i = 0; i < rowDefinitions.Count; i++)
            {
                rowDefinitions[i].Height = new GridLength(e.NewValue);
            }
            for (int i = 0; i < images.Count; i++)
            {
                images[i].Height = e.NewValue;
                images[i].Width = e.NewValue;
            }
            SettingsVariables.height = e.NewValue;
            HeightLabel.Content = "Row height: " + SettingsVariables.height;
        }

        private void FontSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(SaveButton.IsEnabled == false)
            {
                SaveButton.IsEnabled = true;
                SaveButtonShadow.ShadowDepth = 5;
            }

            switch ((int)e.NewValue)
            {
                case 1:
                    SettingsVariables.fontSize = 1;
                    FontSizeLabel.Content = "Font size: 1";
                    for(int i = 0; i < titleTexts.Count; i++)
                    {
                        titleTexts[i].FontSize = 10;
                    }
                    break;
                case 2:
                    SettingsVariables.fontSize = 2;
                    FontSizeLabel.Content = "Font size: 2";
                    for (int i = 0; i < titleTexts.Count; i++)
                    {
                        titleTexts[i].FontSize = 12;
                    }
                    break;
                case 3:
                    SettingsVariables.fontSize = 3;
                    FontSizeLabel.Content = "Font size: 3";
                    for (int i = 0; i < titleTexts.Count; i++)
                    {
                        titleTexts[i].FontSize = 14;
                    }
                    break;
            }
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

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
        public double height;
        public Settings(List<RowDefinition> rowDefinitions, List<Image> images, double height)
        {
            this.rowDefinitions = rowDefinitions;
            this.images = images;
            this.height = height;
            InitializeComponent();
            Debug.WriteLine(HeightSlider.Value);
            HeightSlider.Value = SettingsVariables.height;
            HeightLabel.Content = "Row height: " + SettingsVariables.height;
            HeightSlider.ValueChanged += Slider_ValueChanged;
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
            Close();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Debug.WriteLine("Value changed to: " + e.NewValue);
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

        private void Save(object sender, RoutedEventArgs e)
        {
            SettingsVariables.SaveSettings(SettingsVariables.height);
        }
    }
}

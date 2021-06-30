using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1
{
    static class Functions
    {
        static Window window;
        static Grid livePanelGrid;
        static Grid offlinePanelGrid;
        static Label titleLabel;

        public static void LoadElements(Window AppWindow, Grid LivePanelGrid, Grid OfflinePanelGrid, Label TitleLabel)
        {
            window = AppWindow;
            livePanelGrid = LivePanelGrid;
            offlinePanelGrid = OfflinePanelGrid;
            titleLabel = TitleLabel;
        }

        public static void PaintUI(string tag)
        {
            if (tag == "themeColor")
            {
                // Paints the MainWindow.
                window.Background = new SolidColorBrush(SetColor(SettingsVariables.themeColor));

                // Paints the StackPanels in the live streamer list.
                for (int i = 0; i < livePanelGrid.RowDefinitions.Count; i++)
                {
                    (livePanelGrid.Children[i] as StackPanel).Background = new SolidColorBrush(Grid.GetRow(livePanelGrid.Children[i]) % 2 == 0 ? SetColor(SettingsVariables.themeColor) : SetColor(SettingsVariables.themeColor2));
                }
                // Paints the StackPanels in the offline streamer list.
                for (int i = 0; i < offlinePanelGrid.RowDefinitions.Count; i++)
                {
                    (offlinePanelGrid.Children[i] as StackPanel).Background = new SolidColorBrush(Grid.GetRow(offlinePanelGrid.Children[i]) % 2 == 0 ? SetColor(SettingsVariables.themeColor) : SetColor(SettingsVariables.themeColor2));
                }
            }
            else if (tag == "fontColor")
            {
                // Paints the Font of the Window title.
                titleLabel.Foreground = new SolidColorBrush(SetColor(SettingsVariables.fontColor));

                // Paints the Font in the live streamer list.
                for (int i = 0; i < livePanelGrid.Children.Count; i++)
                {
                    (((((livePanelGrid.Children[i] as StackPanel).Children[0] as Grid).Children[1] as Grid).Children[0] as WrapPanel).Children[0] as Label).Foreground = new SolidColorBrush(SetColor(SettingsVariables.fontColor));
                    ((((((livePanelGrid.Children[i] as StackPanel).Children[0] as Grid).Children[1] as Grid).Children[1] as WrapPanel).Children[0] as Label).Content as TextBlock).Foreground = new SolidColorBrush(SetColor(SettingsVariables.fontColor2));
                }
                // Paints the Font in the offline streamer list.
                for (int i = 0; i < offlinePanelGrid.Children.Count; i++)
                {
                    (((((offlinePanelGrid.Children[i] as StackPanel).Children[0] as Grid).Children[1] as Grid).Children[0] as WrapPanel).Children[0] as Label).Foreground = new SolidColorBrush(SetColor(SettingsVariables.fontColor));
                    ((((((offlinePanelGrid.Children[i] as StackPanel).Children[0] as Grid).Children[1] as Grid).Children[1] as WrapPanel).Children[0] as Label).Content as TextBlock).Foreground = new SolidColorBrush(SetColor(SettingsVariables.fontColor2));
                }
            }


        }

        public static Color SetColor(string hexColor)
        {
            Color color = (Color)ColorConverter.ConvertFromString(hexColor);
            return color;
        }
    }
}

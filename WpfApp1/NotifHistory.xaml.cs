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
    /// Interaction logic for NotifHistory.xaml
    /// </summary>
    public partial class NotifHistory : Window
    {
        public NotifHistory()
        {
            InitializeComponent();
            LoadNotifs();
            if(Notifications.NotificationList.Count > 0)
            {
                ZeroNotifsLabel.Visibility = Visibility.Hidden;
                ClearButton.Visibility = Visibility.Visible;
            }
            Notifications.NotificationList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(
                delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
                {
                    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        LoadNewNotif(Notifications.NotificationList.Count - 1);
                        if(ZeroNotifsLabel.Visibility == Visibility.Visible)
                        {
                            ZeroNotifsLabel.Visibility = Visibility.Hidden;
                            ClearButton.Visibility = Visibility.Visible;
                        }
                    }
                }
            );
        }

        private void LoadNewNotif(int i)
        {
            Notification Notif = Notifications.NotificationList[i];

            // Labels
            Label Streamer = new()
            {
                Content = Notif.StreamerName,
                FontSize = 18,
                FontWeight = FontWeights.DemiBold,
                Foreground = new SolidColorBrush(Colors.White),
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top
            };
            Run FillerText = new() { Text = "Went ", Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)) };
            Run Status = new() { Text = Notif.Status, Foreground = new SolidColorBrush(Notif.Status == "Offline" ? Color.FromRgb(87, 87, 87) : Color.FromRgb(233, 25, 22)) };
            TextBlock Text = new();
            Text.Inlines.Add(FillerText);
            Text.Inlines.Add(Status);
            Label StatusLabel = new()
            {
                Content = Text,
                FontSize = 14,
                FontWeight = FontWeights.DemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Bottom
            };
            Label TimeStamp = new()
            {
                Content = $"{Notif.TimeStamp.Hour}:{Notif.TimeStamp.Minute}",
                FontSize = 20,
                FontWeight = FontWeights.DemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 0)
            };
            Label HoverText = new()
            {
                Content = "Click to delete",
                FontSize = 12,
                FontWeight = FontWeights.DemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                Visibility = Visibility.Hidden
            };

            // Panel Grid
            ColumnDefinition NotifPanelCol1 = new() { Width = GridLength.Auto };
            ColumnDefinition NotifPanelCol2 = new();
            ColumnDefinition NotifPanelCol3 = new() { Width = GridLength.Auto };
            Grid NotifPanelGrid = new() { Height = 40 };
            NotifPanelGrid.ColumnDefinitions.Add(NotifPanelCol1);
            NotifPanelGrid.ColumnDefinitions.Add(NotifPanelCol2);
            NotifPanelGrid.ColumnDefinitions.Add(NotifPanelCol3);

            // Adding the Labels
            NotifPanelGrid.Children.Add(Streamer);
            NotifPanelGrid.Children.Add(StatusLabel);
            NotifPanelGrid.Children.Add(TimeStamp);
            NotifPanelGrid.Children.Add(HoverText);
            Grid.SetColumn(TimeStamp, 0);
            Grid.SetColumn(Streamer, 1);
            Grid.SetColumn(StatusLabel, 1);
            Grid.SetColumn(HoverText, 2);
            
            // Panel
            StackPanel NotifPanel = new() { Height = 40, VerticalAlignment = VerticalAlignment.Top, Background = new SolidColorBrush(Color.FromRgb(39, 44, 51)), Margin = new Thickness(0, 0, 0, 2), Cursor = Cursors.Hand };
            RowDefinition NotifGridRow = new() { Height = new GridLength(42) };
            NotifPanel.MouseEnter += (sender, e) =>
            {
                (sender as StackPanel).Background = new SolidColorBrush(Color.FromRgb(59, 64, 71));
                (((sender as StackPanel).Children[0] as Grid).Children[3] as Label).Visibility = Visibility.Visible;
            };
            NotifPanel.MouseLeave += (sender, e) =>
            {
                (sender as StackPanel).Background = new SolidColorBrush(Color.FromRgb(39, 44, 51));
                (((sender as StackPanel).Children[0] as Grid).Children[3] as Label).Visibility = Visibility.Hidden;
            };
            NotifPanel.MouseDown += (sender, e) =>
            {
                Notifications.NotificationList.Remove(Notif);
                ((sender as StackPanel).Parent as Grid).Children.Remove((StackPanel)sender);
                ReOrder();
            };
            Grid.SetRow(NotifPanel, i);

            // Adding Panel Grid to Panel
            NotifPanel.Children.Add(NotifPanelGrid);

            
            NotificationsGrid.RowDefinitions.Add(NotifGridRow);
            NotificationsGrid.Children.Add(NotifPanel);
        }

        private void LoadNotifs()
        {
            for(int i = 0; i < Notifications.NotificationList.Count; i++)
            {
                LoadNewNotif(i);
            }
        }

        private void ReOrder()
        {
            NotificationsGrid.RowDefinitions.Clear();
            if (Notifications.NotificationList.Count != 0)
            {
                for (int i = 0; i < Notifications.NotificationList.Count; i++)
                {
                    RowDefinition NotifGridRow = new() { Height = new GridLength(42) };
                    NotificationsGrid.RowDefinitions.Add(NotifGridRow);
                    Grid.SetRow(NotificationsGrid.Children[i], i);
                }
            }
            else
            {
                ZeroNotifsLabel.Visibility = Visibility.Visible;
                ClearButton.Visibility = Visibility.Hidden;
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            IsOpened.NotifsIsOpen = false;
            Close();
        }

        private void Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void AddNotif(object sender, MouseButtonEventArgs e)
        {
            Notifications.AddNotifs("Basse", "Live");
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            Notifications.NotificationList.Clear();
            NotificationsGrid.Children.Clear();
            NotificationsGrid.RowDefinitions.Clear();

            ZeroNotifsLabel.Visibility = Visibility.Visible;
            ClearButton.Visibility = Visibility.Hidden;
        }
    }
}

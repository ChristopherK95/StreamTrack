using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Squirrel;

namespace TwitchTrack
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        UpdateManager manager;
        public UpdateWindow(NuGet.SemanticVersion currentVersion, NuGet.SemanticVersion availableVersion, UpdateManager manager)
        {
            InitializeComponent();
            this.manager = manager;
            CurrentVersionLabel.Content = "Version: " + currentVersion;
            AvailableVersionLabel.Text = "Available: " + availableVersion;
        }

        private async void Update(object sender, RoutedEventArgs e)
        {
            AvailableVersionLabel.Visibility = Visibility.Hidden;
            LoadingCog.Visibility = Visibility.Visible;
            await manager.UpdateApp();
            LoadingCog.Visibility = Visibility.Hidden;
            AvailableVersionLabel.Visibility = Visibility.Visible;
            
            CurrentVersionLabel.Content = manager.CurrentlyInstalledVersion();
            AvailableVersionLabel.Text = "Successfully updated! Restart the application for the update to take effect.";
            AvailableVersionLabel.FontSize = 18;
            AvailableVersionLabel.FontWeight = FontWeights.DemiBold;
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

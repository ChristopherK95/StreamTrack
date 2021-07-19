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
            AvailableVersionLabel.Content = "Available: " + availableVersion;
        }

        private async void Update(object sender, RoutedEventArgs e)
        {
            await manager.UpdateApp();
            CurrentVersionLabel.Content = manager.CurrentlyInstalledVersion();
            AvailableVersionLabel.Content = "Successfully updated! Turn off the application and start it again for the update to take effect.";
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

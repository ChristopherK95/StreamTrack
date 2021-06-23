using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Brushes.
        readonly SolidColorBrush IconWhite = new(Colors.White);
        readonly SolidColorBrush IconGray = new(Colors.Gray);

        // Strings.
        private string path;
        private string authKey;

        // Lists.
        private List<Streamer> Streamers = new();
        private List<Streamer> RecentStreamers = new();
        private List<StreamerSearchResult> SearchResults = new();
        private List<SavedStreamer> SavedStreamers = new();
        private List<Newtonsoft.Json.Linq.JObject> JsonList = new();
        List<RowDefinition> StreamerRowList = new();
        List<Image> ProfileImageList = new();

        private static double _height;
        public double height
        {
            get { return _height; }
            set { _height = value; } 
        }

        public int increment = 0;

        HttpClient httpClient = new();
        private delegate void Load_result_panels_callback(int i);
        Grid StreamerGrid;
        StreamWriter FileWriter;
        DispatcherTimer dt;
        DispatcherTimer RefreshAnimation;
        DispatcherTimer NotificationTimer = new();
        public MainWindow()
        {
            InitializeComponent();
            height = 60;
            LoadTwitchKey();
            StartupStreamerData();
            UpdateStatus();
            dt = new();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += dtTicker;
            dt.Start();

            RefreshAnimation = new();
            RefreshAnimation.Interval = TimeSpan.FromSeconds(1);
            RefreshAnimation.Tick += RefreshTicker;

            NotificationTimer.Interval = TimeSpan.FromSeconds(3);
            NotificationTimer.Tick += NotifTicker;
        }

        public void Notification(string streamer, string status)
        {
            RowDefinition notification = new() { Height = new GridLength(40)};
            NotificationGrid.RowDefinitions.Add(notification);

            ColumnDefinition StreamerNameCol = new() { Width = GridLength.Auto };
            ColumnDefinition TextCol = new() { Width = GridLength.Auto };
            ColumnDefinition StatusCol = new();
            Grid RowGrid = new();
            RowGrid.ColumnDefinitions.Add(StreamerNameCol);
            RowGrid.ColumnDefinitions.Add(TextCol);
            RowGrid.ColumnDefinitions.Add(StatusCol);

            NotificationGrid.Children.Add(RowGrid);
            Grid.SetRow(RowGrid, 0);

            Label StreamerName = new() 
            { 
                Content = streamer,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                Padding = new Thickness(5, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            RowGrid.Children.Add(StreamerName);
            Grid.SetColumn(StreamerName, 0);

            Label Text = new()
            {
                Content = " has gone ",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.White),
                Padding = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            RowGrid.Children.Add(Text);
            Grid.SetColumn(Text, 1);

            Label Status = new()
            {
                Content = status == "live" ? "LIVE" : "OFFLINE",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(status == "live" ? Color.FromRgb(233, 25, 22) : Color.FromRgb(117, 117, 117)),
                Padding = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            RowGrid.Children.Add(Status);
            Grid.SetColumn(Status, 2);
        }
        
        private void dtTicker(object sender, EventArgs e)
        {
            increment += 1;
            TimerLabel.Content = "Updated " + increment + "s ago";
        }

        private void RefreshTicker(object sender, EventArgs e)
        {
            RefreshIcon.Spin = false;
            RefreshAnimation.Stop();
        }

        private void NotifTicker(object sender, EventArgs e)
        {
            NotificationGrid.Children.Clear();
            NotificationGrid.RowDefinitions.Clear();
            NotificationTimer.Stop();
        }
        
        public async void UpdateStatus()
        {
            while (true)
            {
                await Task.Run(() =>
                {
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                });
                RefreshIcon.Spin = true;
                RefreshAnimation.Start();
                RecentStreamers = Streamers.ToList();
                LoadStreamers();
                increment = 0;
            };
        }

        public async void StartupStreamerData()
        {
            path = Environment.CurrentDirectory + "\\StreamersList.json";
            Debug.WriteLine(path);
            if (!File.Exists(path))
            {
                FileWriter = File.CreateText(path);
                FileWriter.Write("");
            }
            else
            {
                using (StreamReader fileReader = File.OpenText(path))
                {
                    string jsonString = fileReader.ReadToEnd();
                    if (jsonString != "")
                    {
                        JsonList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Newtonsoft.Json.Linq.JObject>>(jsonString);
                        SavedStreamers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SavedStreamer>>(jsonString);

                        dynamic fetchedData = await GetStreamerStatus();
                        dynamic objectData = Newtonsoft.Json.JsonConvert.DeserializeObject(fetchedData);
                        dynamic liveStreamers = objectData["data"];

                        foreach (var s in liveStreamers)
                        {
                            string streamerString = Newtonsoft.Json.JsonConvert.SerializeObject(s);
                            Streamer streamer = Newtonsoft.Json.JsonConvert.DeserializeObject<Streamer>(streamerString);
                            Streamers.Add(streamer);
                        }
                        LoadStreamers();
                    }
                }
            }
        }

        public async Task<dynamic> GetStreamerStatus()
        {
            string url = "https://api.twitch.tv/helix/streams?user_id=" + SavedStreamers[0].id;
            if (SavedStreamers.Count() > 1)
            {
                for (int i = 1; i < SavedStreamers.Count(); i++)
                {
                    url += "&user_id=" + SavedStreamers[i].id;
                }
            }

            HttpClient hc = new();
            hc.DefaultRequestHeaders.Add("Authorization", "Bearer " + authKey);
            hc.DefaultRequestHeaders.Add("Client-Id", "p4dvj9r4r5jnih8uq373imda1n2v0j");

            Task<Stream> Result = hc.GetStreamAsync(url);

            Stream vs = await Result;
            StreamReader am = new(vs);

            return await am.ReadToEndAsync();
        }

        // Loads all saved streamers from the save file and checks their live status, then renders them on the grid.
        public async void LoadStreamers()
        {
            dynamic FetchedData = await GetStreamerStatus();
            dynamic ObjectData = Newtonsoft.Json.JsonConvert.DeserializeObject(FetchedData);
            dynamic LiveStreamers = ObjectData["data"];

            if(Streamers.Count > 0)
            {
                Streamers.Clear();
            }
            foreach (dynamic s in LiveStreamers)
            {
                string StreamerString = Newtonsoft.Json.JsonConvert.SerializeObject(s);
                Streamer Streamer = Newtonsoft.Json.JsonConvert.DeserializeObject<Streamer>(StreamerString);
                Streamers.Add(Streamer);
            }
            
            // Checks if there is any streamer(s) that have gone live and/or offline.
            if(RecentStreamers.Count > 0 && !RecentStreamers.SequenceEqual(Streamers))
            {
                dynamic offline = RecentStreamers.Except(Streamers).ToList();
                dynamic online = Streamers.Except(RecentStreamers).ToList();

                for(int i = 0; i < offline.Count; i++)
                {
                    // Creates a notification of the streamer that went offline.
                    Notification(offline[i].user_name, "");
                }
                for (int i = 0; i < online.Count; i++)
                {
                    // Creates a notification of the streamer that went live.
                    Notification(online[i].user_name, "live");
                }
                NotificationTimer.Start();
            }

            StreamerPanel.Children.Clear();
            
            StreamerGrid = new() { Name = "StreamerGrid", Height = Double.NaN, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Stretch };
            for (int i = 0; i < SavedStreamers.Count; i++)
            {
                RowDefinition StreamerRow = new() { Height = new GridLength(SettingsVariables.height) };
                StreamerRowList.Add(StreamerRow);
                
                Grid RowColumns = new() { HorizontalAlignment = HorizontalAlignment.Stretch};
                ColumnDefinition ImgCol = new() { Width = GridLength.Auto };
                ColumnDefinition NameCol = new();
                RowColumns.ColumnDefinitions.Add(ImgCol);
                RowColumns.ColumnDefinitions.Add(NameCol);
                StackPanel StreamerRowPanel = new();
                StreamerRowPanel.Children.Add(RowColumns);

                StreamerGrid.Children.Add(StreamerRowPanel);
                Grid.SetRow(StreamerRowPanel, i);

                Image ProfileImg = new()
                {
                    Width = SettingsVariables.height,
                    Height = SettingsVariables.height,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Source = new BitmapImage(new Uri(SavedStreamers[i].thumbnail_url)) { DecodePixelHeight = 60, DecodePixelWidth = 60 }
                };
                ProfileImageList.Add(ProfileImg);
                RowColumns.Children.Add(ProfileImg);
                Grid.SetColumn(ProfileImg, 0);

                Label Name = new()
                {
                    Content = SavedStreamers[i].name,
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.White),
                    Width = double.NaN,
                    Padding = new Thickness(5, 2, 0, 0)
                };

                FontAwesome.WPF.FontAwesome Live = new()
                {
                    Icon = FontAwesome.WPF.FontAwesomeIcon.Circle,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(87, 87, 87)),
                    Width = 20,
                    Margin = new Thickness(0, 10, 0, 0),
                    ToolTip = "Live"
                };

                TextBlock TitleContent = new()
                {
                    FontSize = 12,
                    FontFamily = new FontFamily("Arial Black"),
                    Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                Label Title = new()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Padding = new Thickness(5, 0, 0, 0),
                    Content = TitleContent,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    
                };

                for (int j = 0; j < Streamers.Count; j++)
                {
                    if(SavedStreamers[i].id == Streamers[j].user_id)
                    {
                        TitleContent.Text = Streamers[j].title;
                        TitleContent.ToolTip = Streamers[j].title;
                        Live.Foreground = new SolidColorBrush(Color.FromRgb(233, 25, 22));
                        StreamerRow.Tag = "live";
                    }
                }
                
                Grid Rows = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
                RowDefinition UpperRow = new() ;
                RowDefinition LowerRow = new() ;
                Rows.RowDefinitions.Add(UpperRow);
                Rows.RowDefinitions.Add(LowerRow);

                WrapPanel TextPanel = new()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                Rows.Children.Add(TextPanel);
                Grid.SetRow(TextPanel, 0);

                WrapPanel TitlePanel = new()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                Rows.Children.Add(TitlePanel);
                Grid.SetRow(TitlePanel, 1);

                TextPanel.Children.Add(Name);
                TextPanel.Children.Add(Live);

                TitlePanel.Children.Add(Title);

                RowColumns.Children.Add(Rows);
                Grid.SetColumn(Rows, 1);

                StreamerGrid.RowDefinitions.Add(StreamerRow);
            }
            int order = 0;
            for (int i = 0; i < SavedStreamers.Count; i++)
            {
                if (StreamerGrid.RowDefinitions[i].Tag != null && StreamerGrid.RowDefinitions[i].Tag.ToString() == "live")
                {
                    Grid.SetRow(StreamerGrid.Children[i], order);
                    order++;
                }
            }
            for(int i = 0; i < SavedStreamers.Count; i++)
            {
                if (StreamerGrid.RowDefinitions[i].Tag == null)
                {
                    Grid.SetRow(StreamerGrid.Children[i], order);
                    order++;
                }
            }

            for(int i = 0; i < StreamerGrid.Children.Count; i++)
            {
                StreamerGrid.Children[i].SetValue(BackgroundProperty, new SolidColorBrush(Grid.GetRow(StreamerGrid.Children[i]) % 2 == 0 ? Color.FromRgb(28, 32, 38) : Color.FromRgb(43, 47, 54)));
            }

            StreamerPanel.Children.Add(StreamerGrid);
        }

        public void LoadTwitchKey()
        {
            string url = "https://id.twitch.tv/oauth2/token?client_id=p4dvj9r4r5jnih8uq373imda1n2v0j&client_secret=r5lartsu3ag7i7nc59owx30u9dste6&grant_type=client_credentials";
            HttpWebRequest WebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            WebRequest.Method = "POST";
            dynamic wResponse = WebRequest.GetResponse().GetResponseStream();
            StreamReader Reader = new(wResponse);
            dynamic res = Reader.ReadToEnd();
            Reader.Close();
            wResponse.Close();

            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(res);
            authKey = json["access_token"];
        }

        public async Task<dynamic> FetchData(string SearchName)
        {
            string url = "https://api.twitch.tv/helix/search/channels?query=" + SearchName;
            HttpClient Client = new();
            Client.DefaultRequestHeaders.Add("Authorization", "Bearer " +  authKey);
            Client.DefaultRequestHeaders.Add("Client-Id", "p4dvj9r4r5jnih8uq373imda1n2v0j");
            
            Task<Stream> Result = Client.GetStreamAsync(url);

            Stream StreamResult = await Result;
            StreamReader StreamReaderResult = new(StreamResult);
            
            return await StreamReaderResult.ReadToEndAsync();
        }

        /////////////////////////
        // EVENTHANDLERS START //
        /////////////////////////
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Visibility == Visibility.Hidden)
            {
                SearchBox.Visibility = Visibility.Visible;
                SearchResultsPanel.Visibility = Visibility.Visible;
                AddButton.Icon = FontAwesome.WPF.FontAwesomeIcon.Times;
                SearchBox.Focus();

                Storyboard SlideIn = Resources["SlideIn"] as Storyboard;
                SearchResultsPanel.BeginStoryboard(SlideIn);

            }
            else if (SearchBox.Visibility == Visibility.Visible)
            {
                SearchBox.Visibility = Visibility.Hidden;
                SearchResultsPanel.Visibility = Visibility.Hidden;
                AddButton.Icon = FontAwesome.WPF.FontAwesomeIcon.Search;

                Storyboard SlideOut = Resources["SlideOut"] as Storyboard;
                SearchResultsPanel.BeginStoryboard(SlideOut);
            }
        }
        private async void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SearchBox.Text != "")
            {
                if (SearchResults.Count > 0)
                {
                    SearchResults.Clear();
                    SearchResultsGrid.Children.Clear();
                    SearchResultsGrid.RowDefinitions.Clear();
                }

                string searchName = SearchBox.Text;

                dynamic data = await FetchData(searchName);
                dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(data);
                dynamic jsonStreamers = json["data"];

                foreach (dynamic s in jsonStreamers)
                {
                    string jsonStreamer = Newtonsoft.Json.JsonConvert.SerializeObject(s);

                    StreamerSearchResult Streamer = Newtonsoft.Json.JsonConvert.DeserializeObject<StreamerSearchResult>(jsonStreamer);
                    SearchResults.Add(Streamer);
                }

                LoadCog.Visibility = Visibility.Visible;
                for (int i = 0; i < SearchResults.Count; i++)
                {
                    await Task.Run(() =>
                    {
                        Dispatcher.BeginInvoke(new Load_result_panels_callback(AddResult), new object[] { i });
                    });
                }
                LoadCog.Visibility = Visibility.Hidden;
                SearchResultsGrid.Visibility = Visibility.Visible;
            }
        }
        public void AddResult(int i)
        {
            Grid PanelGrid = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
            ColumnDefinition ImgCol = new() { Width = GridLength.Auto };
            ColumnDefinition NameCol = new();
            ColumnDefinition AddCol = new() { Width = new GridLength(50) };
            PanelGrid.ColumnDefinitions.Add(ImgCol);
            PanelGrid.ColumnDefinitions.Add(NameCol);
            PanelGrid.ColumnDefinitions.Add(AddCol);
            
            Image ProfileImg = new() { Source = new BitmapImage(new Uri(SearchResults[i].thumbnail_url)) { DecodePixelHeight = 50, DecodePixelWidth = 50 }, Width = 50, HorizontalAlignment = HorizontalAlignment.Left };
            PanelGrid.Children.Add(ProfileImg);
            Grid.SetColumn(ProfileImg, 0);

            Label Name = new() { Content = SearchResults[i].display_name, Foreground = new SolidColorBrush(Colors.White), FontSize = 22, VerticalContentAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Stretch };
            PanelGrid.Children.Add(Name);
            Grid.SetColumn(Name, 1);

            

            FontAwesome.WPF.FontAwesome Add = new() 
            {
                FontSize = 40,
                Foreground = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };

            if (SavedStreamers.Exists(item => item.id == SearchResults[i].id))
            {
                Add.Icon = FontAwesome.WPF.FontAwesomeIcon.Check;
                Add.Foreground = new SolidColorBrush(Color.FromArgb(255, 62, 194, 64));
            }
            else
            {
                Add.Icon = FontAwesome.WPF.FontAwesomeIcon.Plus;
                Add.MouseEnter += (sender, e) =>
                {
                    if (Add.Icon == FontAwesome.WPF.FontAwesomeIcon.Plus)
                    {
                        Add.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    }
                };
                Add.MouseLeave += (sender, e) =>
                {
                    if (Add.Icon == FontAwesome.WPF.FontAwesomeIcon.Plus)
                    {
                        Add.Foreground = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
                    }
                };
                Add.MouseLeftButtonUp += (sender, e) =>
                {
                    SaveStreamer(SearchResults[i]);
                    LoadStreamers();
                    Add.Icon = FontAwesome.WPF.FontAwesomeIcon.Check;
                    Add.Foreground = new SolidColorBrush(Color.FromArgb(255, 62, 194, 64));
                };
            }
            
            PanelGrid.Children.Add(Add);
            Grid.SetColumn(Add, 2);

            StackPanel ResultPanel = new() { HorizontalAlignment = HorizontalAlignment.Stretch };

            if (i % 2 == 0)
            {
                ResultPanel.Background = new SolidColorBrush(Color.FromRgb(28, 32, 38));
            }
            else
            {
                ResultPanel.Background = new SolidColorBrush(Color.FromRgb(43, 47, 54));
            }
            ResultPanel.Children.Add(PanelGrid);
            PanelGrid.Width = ResultPanel.Width;
            Grid.SetRow(ResultPanel, i);

            RowDefinition ResultRow = new() { Height = new GridLength(50) };

            SearchResultsGrid.RowDefinitions.Add(ResultRow);
            SearchResultsGrid.Children.Add(ResultPanel);
        }

        public void SaveStreamer(StreamerSearchResult result)
        {
            Newtonsoft.Json.Linq.JObject StreamerJson = new(
                new Newtonsoft.Json.Linq.JProperty("id", result.id),
                new Newtonsoft.Json.Linq.JProperty("name", result.display_name),
                new Newtonsoft.Json.Linq.JProperty("thumbnail_url", result.thumbnail_url));

            JsonList.Add(StreamerJson);
            SavedStreamers.Add(new SavedStreamer(result.id, result.display_name, result.thumbnail_url));

            using (FileWriter = File.CreateText(path))
            {
                Newtonsoft.Json.JsonSerializer Serializer = new();
                Serializer.Serialize(FileWriter, JsonList);
            }
        }

        private void AddButton_MouseEnter(object sender, MouseEventArgs e)
        {
            AddButton.Foreground = IconWhite;
        }

        private void AddButton_MouseLeave(object sender, MouseEventArgs e)
        {
            AddButton.Foreground = IconGray;
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void RefreshIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }


        private void Settings(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new(StreamerRowList, ProfileImageList, height);
            settingsWindow.Show();
        }
    }
}
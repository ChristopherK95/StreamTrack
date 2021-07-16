using StreamTrack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
        readonly SolidColorBrush LiveBrush = new(Color.FromRgb(233, 25, 22));
        readonly SolidColorBrush OfflineBrush = new(Color.FromRgb(87, 87, 87));

        // Strings.
        private string path;
        private string authKey;

        // Lists.
        readonly private List<Streamer> Streamers = new();
        private List<Streamer> RecentStreamers = new();
        readonly private List<StreamerSearchResult> SearchResults = new();
        private List<SavedStreamer> SavedStreamers = new();
        private List<Newtonsoft.Json.Linq.JObject> JsonList = new();
        readonly List<RowDefinition> StreamerRowList = new();
        readonly List<Image> ProfileImageList = new();
        readonly List<TextBlock> TitleTextList = new();
        readonly List<Grid> PanelList = new();
        readonly List<StackPanel> RowPanel = new();

        public int increment = 0;

        readonly HttpClient httpClient = new();
        private delegate void Load_result_panels_callback(int i);
        Grid StreamerGrid;
        Grid OfflineGrid;
        StreamWriter FileWriter;
        DispatcherTimer dt;
        DispatcherTimer RefreshAnimation;
        readonly DispatcherTimer NotificationTimer = new();

        private readonly SoundPlayer _soundPlayer = new();

        readonly Storyboard SlideDown;
        readonly Storyboard SlideUp;

        RightClickWindow info;

        int StreamerSize;
        public MainWindow()
        {
            InitializeComponent();
            CurrentTokenLabel.Content = SettingsVariables.authKey;

            SlideDown = Resources["SlideDown"] as Storyboard;
            SlideUp = Resources["SlideUp"] as Storyboard;
            SlideUp.Completed += (sender, e) =>
            {
                SlideMenu.Visibility = Visibility.Hidden;
            };

            if (SettingsVariables.authKey == "empty")
            {
                TokenView.Visibility = Visibility.Visible;
                TokenStatus.Text = "You don't have a registered Authorization Token!";
            }
            else
            {
                authKey = SettingsVariables.authKey;
                ValidateToken();
            }
        }

        private async void ValidateToken()
        {
            HttpClient hc = new();
            hc.DefaultRequestHeaders.Add("Authorization", "OAuth " + authKey);
            string url = "https://id.twitch.tv/oauth2/validate";

            Task<Stream> Result = hc.GetStreamAsync(url);
            Stream vs;
            try
            {
                vs = await Result;
                
            }
            catch(Exception e)
            {
                if (e.ToString().Contains("Unauthorized"))
                {
                    Run r = new() { Text = "Your token has expired. Generate a new token to keep using StreamTrack." };
                    TokenStatus.Inlines.Add(r);
                    TokenView.Visibility = Visibility.Visible;
                }
                vs = null;
            }
            if(vs != null)
            {
                StreamReader am = new(vs);
                dynamic FetchedData = await am.ReadToEndAsync();
                dynamic ObjectData = Newtonsoft.Json.JsonConvert.DeserializeObject(FetchedData);
                dynamic TokenLifeSpan = ObjectData["expires_in"];

                if (TokenLifeSpan < 86400)
                {
                    Run r1 = new() { Text = "Your token is about to expire, preventing StreamTrack from updating you of your favourite streamers." };
                    LineBreak lb = new();
                    Run r2 = new() { Text = "Might be a good idea to get a new token." };
                    TokenStatus.Inlines.Add(r1);
                    TokenStatus.Inlines.Add(lb);
                    TokenStatus.Inlines.Add(r2);

                    TokenView.Visibility = Visibility.Visible;
                }
                else
                {
                    Start();
                }
            }
        }

        private void Start()
        {
            authKey = SettingsVariables.authKey;
            StartupStreamerData();
            UpdateStatus();

            _soundPlayer.Stream = FileStore.Resource1.NotificationSound;
            _soundPlayer.Load();

            dt = new();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += DtTicker;
            dt.Start();

            RefreshAnimation = new();
            RefreshAnimation.Interval = TimeSpan.FromSeconds(1);
            RefreshAnimation.Tick += RefreshTicker;

            NotificationTimer.Interval = TimeSpan.FromSeconds(3);
            NotificationTimer.Tick += NotifTicker;

            TitleLabel.Foreground = new SolidColorBrush(SetColor(SettingsVariables.fontColor));
            StreamTrackWindow.Background = new SolidColorBrush(SetColor(SettingsVariables.themeColor));
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
        
        private void DtTicker(object sender, EventArgs e)
        {
            increment += 1;
            TimerLabel.Content = "Updated " + increment + "s ago";
        }

        private void RefreshTicker(object sender, EventArgs e)
        {
            RefreshIcon.Spin = false;
            RefreshAnimation.Stop();
            RefreshIcon.Visibility = Visibility.Collapsed;
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
                await Task.Delay(TimeSpan.FromMinutes(1));
                RefreshIcon.Visibility = Visibility.Visible;
                RefreshIcon.Spin = true;
                RefreshAnimation.Start();
                RecentStreamers = Streamers.ToList();
                UpdateStreamerUI();
                increment = 0;
            };
        }

        public async void UpdateStreamerUI()
        {
            if(SavedStreamers.Count == 0)
            {
                return;
            }
            dynamic FetchedData = await GetStreamerStatus();
            dynamic ObjectData = Newtonsoft.Json.JsonConvert.DeserializeObject(FetchedData);
            dynamic LiveStreamers = ObjectData["data"];

            if (Streamers.Count > 0)
            {
                Streamers.Clear();
            }
            foreach (dynamic s in LiveStreamers)
            {
                string StreamerString = Newtonsoft.Json.JsonConvert.SerializeObject(s);
                Streamer Streamer = Newtonsoft.Json.JsonConvert.DeserializeObject<Streamer>(StreamerString);
                Streamers.Add(Streamer);
            }
            
            List<List<Streamer>> offset = GetStatusChange();

            if(offset != null)
            {
                List<Streamer> offline = offset[0];
                List<Streamer> online = offset[1];

                for (int i = 0; i < offline.Count; i++)
                {
                    // Creates a notification of the streamer that went offline.
                    for(int j = 0; j < PanelList.Count; j++)
                    {
                        
                        if(offline[i].user_name == (PanelList[j].Children[0] as WrapPanel).Children[0].GetValue(ContentProperty).ToString())
                        {
                            ((PanelList[j].Children[0] as WrapPanel).Children[1] as FontAwesome5.SvgAwesome).Foreground = OfflineBrush;
                            (((PanelList[j].Children[1] as WrapPanel).Children[0] as Label).Content as TextBlock).Text = "";
                            
                            StackPanel p = RowPanel.Find(panel => (string)panel.Tag == offline[i].user_name);

                            StreamerGrid.Children.Remove(p);
                            OfflineGrid.Children.Add(p);

                            break;
                        }
                    }
                }
                for (int i = 0; i < online.Count; i++)
                {
                    // Creates a notification of the streamer that went live.
                    for (int j = 0; j < PanelList.Count; j++)
                    {
                        if (online[i].user_name == (PanelList[j].Children[0] as WrapPanel).Children[0].GetValue(ContentProperty).ToString())
                        {
                            ((PanelList[j].Children[0] as WrapPanel).Children[1] as FontAwesome5.SvgAwesome).Foreground = LiveBrush;
                            (((PanelList[j].Children[1] as WrapPanel).Children[0] as Label).Content as TextBlock).Text = online[i].title;

                            StackPanel p = RowPanel.Find(panel => (string)panel.Tag == online[i].user_name);

                            OfflineGrid.Children.Remove(p);
                            StreamerGrid.Children.Add(p);

                            break;
                        }
                    }
                }

                ReOrder();
            }
        }

        public void ReOrder()
        {
            StreamerGrid.RowDefinitions.Clear();
            OfflineGrid.RowDefinitions.Clear();
            for(int i = 0; i < StreamerGrid.Children.Count; i++)
            {
                RowDefinition r = new();
                StreamerGrid.RowDefinitions.Add(r);
                Grid.SetRow(StreamerGrid.Children[i], i);
                StreamerGrid.Children[i].SetValue(BackgroundProperty, i % 2 == 0 ? new SolidColorBrush(SetColor(SettingsVariables.themeColor)) : new SolidColorBrush(SetColor(SettingsVariables.themeColor2)));
            }
            for (int i = 0; i < OfflineGrid.Children.Count; i++)
            {
                RowDefinition r = new();
                OfflineGrid.RowDefinitions.Add(r);
                Grid.SetRow(OfflineGrid.Children[i], i);
                OfflineGrid.Children[i].SetValue(BackgroundProperty, i % 2 == 0 ? new SolidColorBrush(SetColor(SettingsVariables.themeColor)) : new SolidColorBrush(SetColor(SettingsVariables.themeColor2)));
            }
        }

        public async void StartupStreamerData()
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TwitchTrack");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TwitchTrack", "StreamersList.json");

            if (!File.Exists(path))
            {
                FileWriter = File.CreateText(path);
                FileWriter.Write("");
                FileWriter.Close();
            }
            else
            {
                using StreamReader fileReader = File.OpenText(path);
                string jsonString = fileReader.ReadToEnd();
                if (jsonString != "[]")
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

        public async Task<dynamic> GetStreamerStatus()
        {
            string url = "https://api.twitch.tv/helix/streams?user_id=" + SavedStreamers[0].id;
            if (SavedStreamers.Count > 1)
            {
                for (int i = 1; i < SavedStreamers.Count; i++)
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

        public List<List<Streamer>> GetStatusChange()
        {
            if (RecentStreamers.Count > 0 && !RecentStreamers.SequenceEqual(Streamers))
            {
                List<Streamer> offline = RecentStreamers.Except(Streamers).ToList();
                List<Streamer> online = Streamers.Except(RecentStreamers).ToList();

                for (int i = 0; i < offline.Count; i++)
                {
                    // Creates a notification of the streamer that went offline.
                    Notification(offline[i].user_name, "");
                    Notifications.AddNotifs(offline[i].user_name, "Offline");
                }
                for (int i = 0; i < online.Count; i++)
                {
                    // Creates a notification of the streamer that went live.
                    Notification(online[i].user_name, "live");
                    Notifications.AddNotifs(online[i].user_name, "Live");
                }
                _soundPlayer.Play();
                NotificationTimer.Start();
                List<List<Streamer>> offset = new();
                
                offset.Add(offline);
                offset.Add(online);
                return offset;
            }
            else
            {
                return null;
            }
            
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

            StreamerGrid = new() { Name = "StreamerGrid", Height = Double.NaN, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Stretch };
            OfflineGrid = new() { Name = "OfflineGrid", HorizontalAlignment = HorizontalAlignment.Stretch };

            for (int i = 0; i < SavedStreamers.Count; i++)
            {
                RowDefinition StreamerRow = new() { Height = new GridLength(SettingsVariables.height), Tag = SavedStreamers[i].name };
                StreamerRowList.Add(StreamerRow);

                Grid RowColumns = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
                ColumnDefinition ImgCol = new() { Width = GridLength.Auto };
                ColumnDefinition NameCol = new();
                RowColumns.ColumnDefinitions.Add(ImgCol);
                RowColumns.ColumnDefinitions.Add(NameCol);
                StackPanel StreamerRowPanel = new() { Tag = SavedStreamers[i].name };
                RowPanel.Add(StreamerRowPanel);
                StreamerRowPanel.Children.Add(RowColumns);

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
                    FontSize = 30,
                    FontWeight = FontWeights.Bold,
                    FontFamily = new FontFamily("Consolas"),
                    Foreground = new SolidColorBrush(SetColor(SettingsVariables.fontColor)),
                    Width = double.NaN,
                    Padding = new Thickness(5, 2, 0, 0),
                    Cursor = Cursors.Hand
                };

                Name.MouseEnter += Hover;
                Name.MouseLeave += LeaveHover;
                Name.MouseUp += LinkToStream;

                FontAwesome5.SvgAwesome Live = new()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_Circle,
                    Height = 15,
                    Foreground = OfflineBrush,
                    Width = 15,
                    Margin = new Thickness(5, 0, 0, 0),
                    ToolTip = "Offline"
                };

                TextBlock TitleContent = new()
                {
                    Text = "",
                    FontSize = SettingsVariables.fontSize == 1 ? 10 : SettingsVariables.fontSize == 2 ? 12 : 14,
                    FontFamily = new FontFamily("Arial Black"),
                    Foreground = new SolidColorBrush(SetColor(SettingsVariables.fontColor2)),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                TitleTextList.Add(TitleContent);

                Label Title = new()
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Padding = new Thickness(5, 0, 0, 0),
                    Content = TitleContent,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Bottom
                };

                for (int j = 0; j < Streamers.Count; j++)
                {
                    if(SavedStreamers[i].id == Streamers[j].user_id)
                    {
                        TitleContent.Text = Streamers[j].title;
                        TitleContent.ToolTip = Streamers[j].title;
                        Live.Foreground = LiveBrush;
                        Live.ToolTip = "Live";
                    }
                }
                
                Grid Rows = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
                PanelList.Add(Rows);
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
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top
                };
                Rows.Children.Add(TitlePanel);
                Grid.SetRow(TitlePanel, 1);

                TextPanel.Children.Add(Name);
                TextPanel.Children.Add(Live);

                TitlePanel.Children.Add(Title);
                
                RowColumns.Children.Add(Rows);
                Grid.SetColumn(Rows, 1);

                StreamerRowPanel.MouseUp += (sender, e) =>
                {
                    if (e.ChangedButton == MouseButton.Right)
                    {
                        info = new(sender as StackPanel, this, SavedStreamers, Streamers, JsonList, path);
                        info.Show();
                        Mouse.Capture(this, CaptureMode.None);
                        AddHandler();
                    }
                };
                if (TitleContent.Text != "")
                {
                    StreamerGrid.Children.Add(StreamerRowPanel);
                    Grid.SetRow(StreamerRowPanel, StreamerGrid.RowDefinitions.Count);
                    StreamerGrid.RowDefinitions.Add(StreamerRow);
                }
                else
                {
                    OfflineGrid.Children.Add(StreamerRowPanel);
                    Grid.SetRow(StreamerRowPanel, OfflineGrid.RowDefinitions.Count);
                    OfflineGrid.RowDefinitions.Add(StreamerRow);
                }
            }

            for (int i = 0; i < StreamerGrid.Children.Count; i++)
            {
                StreamerGrid.Children[i].SetValue(BackgroundProperty, new SolidColorBrush(Grid.GetRow(StreamerGrid.Children[i]) % 2 == 0 ? SetColor(SettingsVariables.themeColor) : SetColor(SettingsVariables.themeColor2)));
            }
            for (int i = 0; i < OfflineGrid.Children.Count; i++)
            {
                OfflineGrid.Children[i].SetValue(BackgroundProperty, new SolidColorBrush(Grid.GetRow(OfflineGrid.Children[i]) % 2 == 0 ? SetColor(SettingsVariables.themeColor) : SetColor(SettingsVariables.themeColor2)));
            }

            StreamerPanel.Children.Add(StreamerGrid);
            Grid.SetRow(StreamerGrid, 1);
            StreamerPanel.Children.Add(OfflineGrid);
            Grid.SetRow(OfflineGrid, 3);

            Functions.LoadElements(StreamerGrid, OfflineGrid);
            StreamerSize = SavedStreamers.Count;
        }

        private void Hover(object sender, EventArgs e)
        {
            (sender as Label).Foreground = new SolidColorBrush(SetColor("#FF39AC63"));
        }

        private void LeaveHover(object sender, EventArgs e)
        {
            (sender as Label).Foreground = new SolidColorBrush(SetColor(SettingsVariables.fontColor));
        }

        private void LinkToStream(object sender, EventArgs e)
        {
            string name = (sender as Label).Content.ToString();
            Process.Start(new ProcessStartInfo($"https://twitch.tv/{name}/") { UseShellExecute = true });
        }

        public static Color SetColor(string hexColor)
        {
            Color color = (Color)ColorConverter.ConvertFromString(hexColor);
            return color;
        }

        public async Task<dynamic> FetchData(string SearchName)
        {
            string url = "https://api.twitch.tv/helix/search/channels?query=" + SearchName;
            HttpClient Client = new();
            Client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authKey);
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
                if (StreamerSize > SavedStreamers.Count)
                {
                    for (int i = 0; i < SearchResultsGrid.Children.Count; i++)
                    {
                        if ((((SearchResultsGrid.Children[i] as StackPanel).Children[0] as Grid).Children[2] as FontAwesome5.SvgAwesome).Icon == FontAwesome5.EFontAwesomeIcon.Solid_Check
                            &&
                            !Streamers.Exists(streamer => streamer.user_name == (((SearchResultsGrid.Children[i] as StackPanel).Children[0] as Grid).Children[1] as Label).Content.ToString()))
                        {
                            (((SearchResultsGrid.Children[i] as StackPanel).Children[0] as Grid).Children[2] as FontAwesome5.SvgAwesome).Icon = FontAwesome5.EFontAwesomeIcon.Solid_Plus;
                            (((SearchResultsGrid.Children[i] as StackPanel).Children[0] as Grid).Children[2] as FontAwesome5.SvgAwesome).Foreground = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
                        }
                    }
                    StreamerSize = SavedStreamers.Count;
                }
                
                SearchBox.Visibility = Visibility.Visible;
                SearchResultsPanel.Visibility = Visibility.Visible;
                AddButton.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Times;
                SearchBox.Focus();

                Storyboard SlideIn = Resources["SlideIn"] as Storyboard;
                SearchResultsPanel.BeginStoryboard(SlideIn);

            }
            else if (SearchBox.Visibility == Visibility.Visible)
            {
                SearchBox.Visibility = Visibility.Hidden;
                SearchResultsPanel.Visibility = Visibility.Hidden;
                AddButton.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Plus;

                Storyboard SlideOut = Resources["SlideOut"] as Storyboard;
                SearchResultsPanel.BeginStoryboard(SlideOut);
            }
        }
        private async void Search(object sender, KeyEventArgs e)
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
                LoadCog.Spin = true;
                
                for (int i = 0; i < SearchResults.Count; i++)
                {
                    await Task.Run(() =>
                    {
                        Dispatcher.BeginInvoke(new Load_result_panels_callback(AddResult), new object[] { i });
                    });
                }
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

            Label Name = new() { Content = SearchResults[i].display_name, Foreground = new SolidColorBrush(Colors.White), FontWeight = FontWeights.DemiBold, FontSize = 26, FontFamily = new FontFamily("Consolas"), VerticalContentAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Stretch };
            PanelGrid.Children.Add(Name);
            Grid.SetColumn(Name, 1);

            FontAwesome5.SvgAwesome Add = new()
            {
                Height = 40,
                Foreground = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };

            if (SavedStreamers.Exists(item => item.id == SearchResults[i].id))
            {
                Add.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Check;
                Add.Foreground = new SolidColorBrush(Color.FromArgb(255, 62, 194, 64));
            }
            else
            {
                Add.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Plus;
                Add.MouseEnter += (sender, e) =>
                {
                    if (Add.Icon == FontAwesome5.EFontAwesomeIcon.Solid_Plus)
                    {
                        Add.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    }
                };
                Add.MouseLeave += (sender, e) =>
                {
                    if (Add.Icon == FontAwesome5.EFontAwesomeIcon.Solid_Plus)
                    {
                        Add.Foreground = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
                    }
                };
                Add.MouseLeftButtonDown += (sender, e) =>
                {
                    SaveStreamer(SearchResults[i]);
                    LoadStreamers();
                    Add.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Check;
                    Add.Foreground = new SolidColorBrush(Color.FromArgb(255, 62, 194, 64));
                };
            }
            
            PanelGrid.Children.Add(Add);
            Grid.SetColumn(Add, 2);

            StackPanel ResultPanel = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
            if (i == SearchResults.Count - 1)
            {
                ResultPanel.Loaded += ResultsLoaded;
            }
            
            ResultPanel.Background = i % 2 == 0 ? new SolidColorBrush(Color.FromRgb(28, 32, 38)) : new SolidColorBrush(Color.FromRgb(43, 47, 54));
            ResultPanel.Children.Add(PanelGrid);
            PanelGrid.Width = ResultPanel.Width;
            Grid.SetRow(ResultPanel, i);

            RowDefinition ResultRow = new() { Height = new GridLength(50) };

            SearchResultsGrid.RowDefinitions.Add(ResultRow);
            SearchResultsGrid.Children.Add(ResultPanel);
        }

        public async void ResultsLoaded(object sender, EventArgs e)
        {
            await Task.Delay(500);
            LoadCog.Visibility = Visibility.Hidden;
            SearchResultsGrid.Visibility = Visibility.Visible;
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
                FileWriter.Close();
            }
            
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

        private void Settings(object sender, RoutedEventArgs e)
        {
            if(IsOpened.SettingsIsOpen == false)
            {
                IsOpened.SettingsIsOpen = true;
                Settings SettingsWindow = new(StreamerRowList, ProfileImageList, TitleTextList) { Left = StreamTrackWindow.Left + StreamTrackWindow.ActualWidth + 20, Top = StreamTrackWindow.Top };
                SettingsWindow.Show();
            }
            if (SlideMenu.Opacity == 1)
            {
                SlideMenu.BeginStoryboard(SlideUp);
            }
        }

        private void NotifHistory(object sender, RoutedEventArgs e)
        {
            if(IsOpened.NotifsIsOpen == false)
            {
                IsOpened.NotifsIsOpen = true;
                NotifHistory NotificationsWindow = new() { Left = StreamTrackWindow.Left - 320, Top = StreamTrackWindow.Top };
                NotificationsWindow.Show();
            }
            if (SlideMenu.Opacity == 1)
            {
                SlideMenu.BeginStoryboard(SlideUp);
            }
        }

        private void OpenTokenView(object sender, RoutedEventArgs e)
        {
            TokenView.Visibility = TokenView.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
            if(SlideMenu.Opacity == 1)
            {
                SlideMenu.BeginStoryboard(SlideUp);
            }
        }

        private void GetToken(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://streamtrack-authentication.firebaseapp.com/") { UseShellExecute = true });
        }

        private void PasteToken(object sender, RoutedEventArgs e)
        {
            string userToken = Clipboard.GetText();
            SettingsVariables.authKey = userToken;
            SettingsVariables.SaveSettings();
            CurrentTokenLabel.Content = userToken;
            authKey = userToken;
            TokenView.Visibility = Visibility.Hidden;
            TokenStatus.Inlines.Clear();
            Start();
        }

        private void Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Grid).Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Grid).Background = new SolidColorBrush(Colors.Transparent);
        }

        private void ExtraMenu(object sender, RoutedEventArgs e)
        {
            if (SlideMenu.Visibility == Visibility.Hidden)
            {
                SlideMenu.Visibility = Visibility.Visible;
                SlideMenu.BeginStoryboard(SlideDown);
            }
            else
            {
                SlideMenu.BeginStoryboard(SlideUp);
            }
        }

        private void ImageAwesome_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as FontAwesome5.SvgAwesome).Foreground = new SolidColorBrush(Color.FromRgb(57, 172, 99));
        }

        private void ImageAwesome_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as FontAwesome5.SvgAwesome).Foreground = new SolidColorBrush(Color.FromRgb(89, 89, 89));
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            TokenView.Visibility = Visibility.Hidden;
        }

        private void AddHandler()
        {
            AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(HandleClickOutsideOfControl), true);
        }

        private void HandleClickOutsideOfControl(object sender, MouseButtonEventArgs e)
        {
            info.Close();
            ReleaseMouseCapture();
        }
    }
}
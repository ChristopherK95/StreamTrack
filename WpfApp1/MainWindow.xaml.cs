using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string path;
        List<Streamer> streamers = new();
        List<StreamerSearchResult> SearchResults = new();
        List<SavedStreamer> savedStreamers = new();
        List<Newtonsoft.Json.Linq.JObject> jsonList = new();
        HttpClient httpClient = new();
        string authKey;
        Brush baseColor;
        Brush hoverBrush = new SolidColorBrush(Color.FromRgb(91, 101, 115));
        delegate void Load_result_panels_callback(int i);
        Grid streamerGrid;
        StreamWriter fileWriter;

        public MainWindow()
        {
            InitializeComponent();
            LoadTwitchKey();
            StartupStreamerData();
            UpdateStatus();
        }

        public async void UpdateStatus()
        {
            while (true)
            {
                await Task.Run(() =>
                {
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                    
                });
                LoadStreamers();
                Debug.WriteLine("Updating Status...");
            };
        }

        public async void StartupStreamerData()
        {
            path = Environment.CurrentDirectory + "\\StreamersList.json";
            Debug.WriteLine(path);
            if (!File.Exists(path))
            {
                fileWriter = File.CreateText(path);
                fileWriter.Write("");
            }
            else
            {
                using (StreamReader fileReader = File.OpenText(path))
                {
                    string jsonString = fileReader.ReadToEnd();
                    if (jsonString != "")
                    {
                        jsonList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Newtonsoft.Json.Linq.JObject>>(jsonString);
                        savedStreamers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SavedStreamer>>(jsonString);

                        dynamic fetchedData = await GetStreamerStatus();
                        dynamic objectData = Newtonsoft.Json.JsonConvert.DeserializeObject(fetchedData);
                        dynamic liveStreamers = objectData["data"];

                        foreach (var s in liveStreamers)
                        {
                            string streamerString = Newtonsoft.Json.JsonConvert.SerializeObject(s);
                            Streamer streamer = Newtonsoft.Json.JsonConvert.DeserializeObject<Streamer>(streamerString);
                            streamers.Add(streamer);
                        }
                        LoadStreamers();
                    }
                }
            }
        }

        public async Task<dynamic> GetStreamerStatus()
        {
            string url = "https://api.twitch.tv/helix/streams?user_id=" + savedStreamers[0].id;
            if (savedStreamers.Count() > 1)
            {
                for (int i = 1; i < savedStreamers.Count(); i++)
                {
                    url += "&user_id=" + savedStreamers[i].id;
                }
            }
            
            HttpClient hc = new();
            hc.DefaultRequestHeaders.Add("Authorization", "Bearer " + authKey);
            hc.DefaultRequestHeaders.Add("Client-Id", "p4dvj9r4r5jnih8uq373imda1n2v0j");

            Task<Stream> result = hc.GetStreamAsync(url);

            Stream vs = await result;
            StreamReader am = new(vs);
            
            return await am.ReadToEndAsync();
        }

        void stackpanel_MouseEnter(object sender, MouseEventArgs e)
        {
            StackPanel stackpanel = (StackPanel)sender;
            baseColor = stackpanel.Background;
            stackpanel.Background = hoverBrush;
        }

        void stackpanel_MouseLeave(object sender, MouseEventArgs e)
        {
            StackPanel stackpanel = (StackPanel)sender;
            stackpanel.Background = baseColor;
        }

        

        public async void LoadStreamers()
        {
            dynamic fetchedData = await GetStreamerStatus();
            dynamic objectData = Newtonsoft.Json.JsonConvert.DeserializeObject(fetchedData);
            dynamic liveStreamers = objectData["data"];

            if(streamers.Count > 0)
            {
                streamers.Clear();
            }
            foreach (var s in liveStreamers)
            {
                string streamerString = Newtonsoft.Json.JsonConvert.SerializeObject(s);
                Streamer streamer = Newtonsoft.Json.JsonConvert.DeserializeObject<Streamer>(streamerString);
                streamers.Add(streamer);
            }

            streamerPanel.Children.Clear();

            streamerGrid = new() { Name = "StreamerGrid", Height = Double.NaN, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Stretch };
            for (int i = 0; i < savedStreamers.Count; i++)
            {
                RowDefinition streamerRow = new() { Height = new GridLength(60) };
                streamerRow.Name = "streamerRow" + i;
                Grid rowColumns = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
                ColumnDefinition imgCol = new() { Width = GridLength.Auto };
                ColumnDefinition nameCol = new();
                rowColumns.ColumnDefinitions.Add(imgCol);
                rowColumns.ColumnDefinitions.Add(nameCol);
                
                streamerGrid.Children.Add(rowColumns);
                Grid.SetRow(rowColumns, i);

                Image profileImg = new()
                {
                    Width = 60,
                    Height = 60,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Source = new BitmapImage(new Uri(savedStreamers[i].thumbnail_url)) { DecodePixelHeight = 60, DecodePixelWidth = 60}
                };
                rowColumns.Children.Add(profileImg);
                Grid.SetColumn(profileImg, 0);

                Label name = new()
                {
                    Content = savedStreamers[i].name,
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.White),
                    Width = double.NaN,
                    Padding = new Thickness(5, 2, 0, 0)
                };

                FontAwesome.WPF.FontAwesome live = new()
                {
                    Icon = FontAwesome.WPF.FontAwesomeIcon.Circle,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(87, 87, 87)),
                    Width = 20,
                    Margin = new Thickness(0, 10, 0, 0),
                    ToolTip = "Live"
                };

                TextBlock titleContent = new()
                {
                    FontSize = 12,
                    FontFamily = new FontFamily("Arial Black"),
                    Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                Label title = new()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Padding = new Thickness(5, 0, 0, 0),
                    Content = titleContent,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    
                };

                for (int j = 0; j < streamers.Count; j++)
                {
                    if(savedStreamers[i].id == streamers[j].user_id)
                    {
                        titleContent.Text = streamers[j].title;
                        titleContent.ToolTip = streamers[j].title;
                        live.Foreground = new SolidColorBrush(Color.FromRgb(233, 25, 22));
                        streamerRow.Tag = "live";
                    }
                }
                
                Grid rows = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
                RowDefinition upperRow = new() { Height = new GridLength(30) };
                RowDefinition lowerRow = new() { Height = new GridLength(30) };
                rows.RowDefinitions.Add(upperRow);
                rows.RowDefinitions.Add(lowerRow);

                WrapPanel textPanel = new()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                rows.Children.Add(textPanel);
                Grid.SetRow(textPanel, 0);

                WrapPanel titlePanel = new()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                rows.Children.Add(titlePanel);
                Grid.SetRow(titlePanel, 1);
                
                textPanel.Children.Add(name);
                textPanel.Children.Add(live);

                titlePanel.Children.Add(title);

                rowColumns.Children.Add(rows);
                Grid.SetColumn(rows, 1);

                streamerGrid.RowDefinitions.Add(streamerRow);
            }
            int order = 0;
            for (int i = 0; i < savedStreamers.Count; i++)
            {
                if (streamerGrid.RowDefinitions[i].Tag != null && streamerGrid.RowDefinitions[i].Tag.ToString() == "live")
                {
                    Grid.SetRow(streamerGrid.Children[i], order);
                    order++;
                }
            }
            for(int i = 0; i < savedStreamers.Count; i++)
            {
                if (streamerGrid.RowDefinitions[i].Tag == null)
                {
                    Grid.SetRow(streamerGrid.Children[i], order);
                    order++;
                }
            }

            streamerPanel.Children.Add(streamerGrid);
        }

        public void LoadTwitchKey()
        {
            string url = "https://id.twitch.tv/oauth2/token?client_id=p4dvj9r4r5jnih8uq373imda1n2v0j&client_secret=r5lartsu3ag7i7nc59owx30u9dste6&grant_type=client_credentials";
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webRequest.Method = "POST";
            dynamic wResponse = webRequest.GetResponse().GetResponseStream();
            StreamReader reader = new(wResponse);
            dynamic res = reader.ReadToEnd();
            reader.Close();
            wResponse.Close();

            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(res);
            authKey = json["access_token"];
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(SearchBox.Visibility == Visibility.Hidden) 
            {
                SearchBox.Visibility = Visibility.Visible;
                SearchResultsPanel.Visibility = Visibility.Visible;
                AddButton.Icon = FontAwesome.WPF.FontAwesomeIcon.Times;
                SearchBox.Focus();

                Storyboard slideIn = Resources["SlideIn"] as Storyboard;
                SearchResultsPanel.BeginStoryboard(slideIn);
                
            }
            else if(SearchBox.Visibility == Visibility.Visible)
            {
                SearchBox.Visibility = Visibility.Hidden;
                SearchResultsPanel.Visibility = Visibility.Hidden;
                AddButton.Icon = FontAwesome.WPF.FontAwesomeIcon.Search;

                Storyboard slideOut = Resources["SlideOut"] as Storyboard;
                SearchResultsPanel.BeginStoryboard(slideOut);
            }
        }

        private async void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if(SearchResults.Count > 0)
                {
                    SearchResults.Clear();
                    SearchResultsGrid.Children.Clear();
                    SearchResultsGrid.RowDefinitions.Clear();
                }
                
                string SearchName = SearchBox.Text;

                dynamic data = await FetchData(SearchName);
                dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(data);
                dynamic streamers = json["data"];

                foreach (var s in streamers)
                {
                    string JsonStreamer = Newtonsoft.Json.JsonConvert.SerializeObject(s);

                    StreamerSearchResult streamer = Newtonsoft.Json.JsonConvert.DeserializeObject<StreamerSearchResult>(JsonStreamer);
                    SearchResults.Add(streamer);
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

        public async Task<dynamic> FetchData(string SearchName)
        {
            string Url = "https://api.twitch.tv/helix/search/channels?query=" + SearchName;
            HttpClient hc = new();
            hc.DefaultRequestHeaders.Add("Authorization", "Bearer " +  authKey);
            hc.DefaultRequestHeaders.Add("Client-Id", "p4dvj9r4r5jnih8uq373imda1n2v0j");
            
            Task<Stream> result = hc.GetStreamAsync(Url);

            Stream vs = await result;
            StreamReader am = new(vs);
            
            return await am.ReadToEndAsync();
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
                Icon = FontAwesome.WPF.FontAwesomeIcon.Plus, 
                FontSize = 40, 
                Foreground = new SolidColorBrush(Color.FromArgb(100,255,255,255)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };
            Add.MouseEnter += (sender, e) =>
            {
                if(Add.Icon == FontAwesome.WPF.FontAwesomeIcon.Plus)
                {
                    Add.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
            };
            Add.MouseLeave += (sender, e) =>
            {
                if(Add.Icon == FontAwesome.WPF.FontAwesomeIcon.Plus)
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
            Newtonsoft.Json.Linq.JObject streamerJson = new(
                new Newtonsoft.Json.Linq.JProperty("id", result.id),
                new Newtonsoft.Json.Linq.JProperty("name", result.display_name),
                new Newtonsoft.Json.Linq.JProperty("thumbnail_url", result.thumbnail_url));

            jsonList.Add(streamerJson);
            savedStreamers.Add(new SavedStreamer(result.id, result.display_name, result.thumbnail_url));

            using (fileWriter = File.CreateText(path))
            {
                Newtonsoft.Json.JsonSerializer serializer = new();
                serializer.Serialize(fileWriter, jsonList);
            }
        }

        SolidColorBrush iconWhite = new SolidColorBrush(Colors.White);
        SolidColorBrush iconGray = new SolidColorBrush(Colors.Gray);

        private void AddButton_MouseEnter(object sender, MouseEventArgs e)
        {
            AddButton.Foreground = iconWhite;
        }

        private void AddButton_MouseLeave(object sender, MouseEventArgs e)
        {
            AddButton.Foreground = iconGray;
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

    }

    
}

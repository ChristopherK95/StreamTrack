using System.Collections.Generic;

namespace WpfApp1
{
    public class StreamerSearchResult
    {
        public string broadcaster_language { get; set; }
        public string broadcaster_login { get; set; }
        public string display_name { get; set; }
        public string game_id { get; set; }
        public string game_name { get; set; }
        public string id { get; set; }
        public bool is_live { get; set; }
        public List<object> tag_ids { get; set; }
        public string thumbnail_url { get; set; }
        public string title { get; set; }
        public string started_at { get; set; }
    }
}

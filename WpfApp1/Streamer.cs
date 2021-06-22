using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Streamer: IEquatable<Streamer>
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string user_login { get; set; }
        public string user_name { get; set; }
        public string game_id { get; set; }
        public string game_name { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public int viewer_count { get; set; }
        public DateTime started_at { get; set; }
        public string language { get; set; }
        public string thumbnail_url { get; set; }
        public List<string> tag_ids { get; set; }
        public bool is_mature { get; set; }

        public bool Equals(Streamer other)
        {
            if (other is null)
                return false;

            return this.id == other.id;
        }

        public override bool Equals(object obj) => Equals(obj as Streamer);
        public override int GetHashCode() => (id).GetHashCode();
    }
}

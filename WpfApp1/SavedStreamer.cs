using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class SavedStreamer
    {
        public SavedStreamer(string id, string name, string thumbnail_url)
        {
            this.id = id;
            this.name = name;
            this.thumbnail_url = thumbnail_url;
        }

        public string id { get; set; }
        public string name { get; set; }
        public string thumbnail_url { get; set; }
        public bool live { get; set; }
    }
}

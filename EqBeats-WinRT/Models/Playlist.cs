using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EqBeats_WinRT.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class Playlist {
        public Playlist(int id) {
            Id = id;
        }

        [JsonProperty("id")]
        public readonly int Id;
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("author")]
        public User Artist { get; set; }
        [JsonProperty("tracks")]
        public Track[] Tracks { get; set; }
        [JsonProperty("link")]
        public Uri Link { get; set; }
    }
}

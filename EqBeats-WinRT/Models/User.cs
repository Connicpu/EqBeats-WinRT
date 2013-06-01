using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EqBeats_WinRT.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class User {
        public User(int id) {
            Id = id;
        }

        [JsonProperty("id")]
        public readonly int Id;
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("avatar")]
        public Uri Avatar { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("tracks")]
        public Track[] Tracks { get; set; }
        [JsonProperty("playlists")]
        public Playlist[] Playlists { get; set; }
        [JsonProperty("link")]
        public Uri Link { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EqBeats_WinRT.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class Track {
        public Track(int id) {
            Id = id;
        }

        [JsonProperty("id")]
        public readonly int Id;
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("artist")]
        public User Artist { get; set; }
        [JsonProperty("link")]
        public Uri Link { get; set; }
        [JsonProperty("download")]
        public DownloadEndpoints Download { get; set; }
        [JsonProperty("stream")]
        public DownloadEndpoints Stream { get; set; }
    }
}

using System;
using Newtonsoft.Json;

namespace EqBeats_WinRT.Models {
    public class DownloadEndpoints {
        [JsonProperty("art")]
        public Uri Art { get; set; }
        [JsonProperty("opus")]
        public Uri Opus { get; set; }
        [JsonProperty("vorbis")]
        public Uri Vorbis { get; set; }
        [JsonProperty("aac")]
        public Uri Aac { get; set; }
        [JsonProperty("mp3")]
        public Uri Mp3 { get; set; }
        [JsonProperty("original")]
        public Uri Original { get; set; }
    }
}

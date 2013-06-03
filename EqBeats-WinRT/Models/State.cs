using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EqBeats_WinRT.Annotations;
using EqBeats_WinRT.Common;
using EqBeats_WinRT.Pages;
using Newtonsoft.Json;
using Windows.Storage;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EqBeats_WinRT.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class State : INotifyPropertyChanged {
        private const int MODEL_EDITION = 5;
        [JsonProperty("modelEdition")]
        public int ModelEdition;
        public static State AppState { get; private set; }
        public static bool HasState { get { return AppState != null; } }

        public static bool MediaControlInitialized = false;
        public static DispatcherTimer PlayerUpdateTimer;

        public State(int? modelEdition = null) {
            if (modelEdition != null) {
                ModelEdition = (int)modelEdition;
            } else {
                ModelEdition = MODEL_EDITION;
            }
        }

        [JsonProperty]
        public Type CurrentPageType = typeof(Home);

        private SearchQueryModel _searchQuery = new SearchQueryModel();

        [JsonProperty]
        public SearchQueryModel SearchQuery {
            get { return _searchQuery; }
            set {
                if (value == _searchQuery) return;
                _searchQuery = value;
                OnPropertyChanged("SearchQuery");
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class SearchQueryModel : INotifyPropertyChanged {
            private string _submittedQuery;
            private string _currentQuery;
            public event PropertyChangedEventHandler PropertyChanged;

            [JsonProperty]
            public string SubmittedQuery {
                get { return _submittedQuery; }
                set {
                    if (value == _submittedQuery) return;
                    _submittedQuery = value;
                    CurrentQuery = value;
                    OnPropertyChanged();
                }
            }

            [JsonProperty]
            public string CurrentQuery {
                get { return _currentQuery; }
                set {
                    if (value == _currentQuery) return;
                    _currentQuery = value;
                    OnPropertyChanged();
                }
            }

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private NowPlayingState _nowPlaying = new NowPlayingState();

        [JsonProperty]
        public NowPlayingState NowPlaying {
            get { return _nowPlaying; }
            set {
                if (Equals(value, _nowPlaying)) return;
                _nowPlaying = value;
                OnPropertyChanged();
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class NowPlayingState : INotifyPropertyChanged {
            private int _currentSong;
            private Track[] _trackList;
            private TimeSpan _songPosition;
            private Duration _trackDuration;
            public event PropertyChangedEventHandler PropertyChanged;

            [JsonProperty]
            public int CurrentSong {
                get { return _currentSong; }
                set {
                    if (value == _currentSong) return;
                    _currentSong = value;
                    OnPropertyChanged();

                    if (TrackList == null) return;

                    if (CurrentSong < 0 || CurrentSong >= TrackList.Length) {
                        return;
                    }

                    var track = TrackList[CurrentSong];
                    var player = Window.Current.FindMediaElement();

                    player.Source = track.Download.Mp3;
                    player.Play();

                    var nextEvent = NextSongReady;
                    if (nextEvent != null) NextSongReady(track);
                }
            }

            [JsonProperty]
            public Track[] TrackList {
                get { return _trackList; }
                set {
                    if (Equals(value, _trackList)) return;
                    _trackList = value;
                    OnPropertyChanged();

                    if (CurrentSong >= 0 && CurrentSong < value.Length) {
                        OnPropertyChanged("CurrentSong");
                    }
                }
            }

            public Track PlayingTrack {
                get {
                    if (TrackList == null || TrackList.Length < 1) {
                        throw new InvalidOperationException("Cannot select track as there is no playlist loaded");
                    }
                    if (CurrentSong < 0 || CurrentSong >= TrackList.Length) {
                        return null;
                    }
                    return TrackList[CurrentSong];
                }
                set {
                    if (TrackList == null) {
                        throw new InvalidOperationException("Cannot set track as there is no playlist loaded");
                    }
                    var index = Array.FindIndex(TrackList, track => track.Id == value.Id);
                    if (index < 0) {
                        throw new ArgumentException("Specified track does not exist in the current playlist");
                    }
                    CurrentSong = index;
                }
            }

            public TimeSpan SongPosition {
                get { return _songPosition; }
                set {
                    if (value.Equals(_songPosition)) return;
                    _songPosition = value;
                    OnPropertyChanged();
                }
            }

            public Duration TrackDuration {
                get { return _trackDuration; }
                set {
                    if (value.Equals(_trackDuration)) return;
                    _trackDuration = value;
                    OnPropertyChanged();
                }
            }

            public void NextTrack() {
                if (TrackList == null || TrackList.Length < 1) {
                    throw new InvalidOperationException("Cannot find next track as there is no playlist loaded");
                }
                var index = CurrentSong;
                if (++index >= TrackList.Length) { index = 0; }
                CurrentSong = index;
            }

            public void PreviousTrack() {
                if (TrackList == null || TrackList.Length < 1) {
                    throw new InvalidOperationException("Cannot find next track as there is no playlist loaded");
                }
                var index = CurrentSong;
                if (--index <= 0) { index = TrackList.Length - 1; }
                CurrentSong = index;
            }

            public event Action<Track> NextSongReady;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static async Task SaveState() {
            var stateContents = await JsonConvert.SerializeObjectAsync(AppState, Formatting.Indented);

            var storage = ApplicationData.Current.LocalFolder;
            var file = await storage.CreateFileAsync("state.json", CreationCollisionOption.ReplaceExisting);
            using (var fileStream = await file.OpenStreamForWriteAsync())
            using (var writer = new StreamWriter(fileStream, Encoding.UTF8)) {
                await writer.WriteAsync(stateContents);
                await writer.FlushAsync();
            }
        }

        public static async Task LoadState() {
            var storage = ApplicationData.Current.LocalFolder;

            if ((await storage.GetFilesAsync()).All(item => item.Name != "state.json")) {
                AppState = new State();
                return;
            }

            var file = await storage.GetFileAsync("state.json");

            string stateContents;
            using (var fileStream = await file.OpenStreamForReadAsync())
            using (var reader = new StreamReader(fileStream, Encoding.UTF8)) {
                stateContents = await reader.ReadToEndAsync();
            }

            try {
                AppState = await JsonConvert.DeserializeObjectAsync<State>(stateContents);
                if (AppState.ModelEdition < MODEL_EDITION) {
                    AppState = new State();
                }
            } catch {
                AppState = new State();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

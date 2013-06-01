using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EqBeats_WinRT.Annotations;
using EqBeats_WinRT.Pages;
using Newtonsoft.Json;
using Windows.Storage;
using System.Linq;

namespace EqBeats_WinRT.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class State : INotifyPropertyChanged {
        private const int MODEL_EDITION = 2;
        [JsonProperty("modelEdition")]
        public int ModelEdition;
        public static State AppState { get; private set; }
        public static bool HasState { get { return AppState != null; } }

        public State(int? modelEdition = null) {
            if (modelEdition != null) {
                ModelEdition = (int) modelEdition;
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
                OnPropertyChanged();
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
                    OnPropertyChanged1();
                }
            }

            [JsonProperty]
            public string CurrentQuery {
                get { return _currentQuery; }
                set {
                    if (value == _currentQuery) return;
                    _currentQuery = value;
                    OnPropertyChanged1();
                }
            }

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged1([CallerMemberName] string propertyName = null) {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class NowPlayingState : INotifyPropertyChanged {
            public event PropertyChangedEventHandler PropertyChanged;

            public int CurrentSong { get; set; }
            public Track[] TrackList { get; set; }

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged1([CallerMemberName] string propertyName = null) {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static async Task SaveState() {
            var stateContents = await JsonConvert.SerializeObjectAsync(AppState);

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

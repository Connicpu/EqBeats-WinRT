using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EqBeats_WinRT.Models;
using EqBeats_WinRT.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace EqBeats_WinRT.Controls {
    public sealed partial class SearchResults {
        public SearchResults() {
            InitializeComponent();
        }

        private long loadId = 1;

        public static readonly DependencyProperty SearchQueryProperty =
            DependencyProperty.Register("SearchQuery", typeof(string), typeof(SearchResults),
            new PropertyMetadata(default(string), QueryChanged));

        private static async void QueryChanged(DependencyObject depObject, DependencyPropertyChangedEventArgs args) {
            var control = (SearchResults)depObject;
            var search = (string)args.NewValue;

            switch (control.SearchType) {
                case SearchMode.Track:
                    await (control.SearchTracks(search));
                    break;
                case SearchMode.User:
                    await (control.SearchUsers(search));
                    break;
            }
        }

        public async Task SearchTracks(string query) {
            var currLoadId = ++loadId;

            BeginSearch();

            Track[] results;
            try {
                var path = "http://www.eqbeats.org/tracks/search/json?q=" + Uri.EscapeUriString(query);
                var request = WebRequest.CreateHttp(path);
                request.Accept = "application/json";
                using (var response = await request.GetResponseAsync())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream)) {
                    results = await JsonConvert.DeserializeObjectAsync<Track[]>(await reader.ReadToEndAsync());
                }
            } catch {
                results = new Track[0];
            }

            if (currLoadId != loadId) {
                return;
            }

            UpdateResults(results);
        }

        public async Task SearchUsers(string query) {
            var currLoadId = ++loadId;

            BeginSearch();

            User[] results;
            try {
                var path = "http://www.eqbeats.org/users/search/json?q=" + Uri.EscapeUriString(query);
                var request = WebRequest.CreateHttp(path);
                request.Accept = "application/json";
                using (var response = await request.GetResponseAsync())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream)) {
                    results = await JsonConvert.DeserializeObjectAsync<User[]>(await reader.ReadToEndAsync());
                }
            } catch {
                results = new User[0];
            }

            if (currLoadId != loadId) {
                return;
            }

            UpdateResults(results);
        }
        public void BeginSearch() {
            ResultView.ItemsSource = new object[0];
            ResultView.Visibility = Visibility.Collapsed;
            NoResults.Visibility = Visibility.Collapsed;
        }


        public void UpdateResults(Array results) {
            results = results.Cast<object>().Take(50).ToArray();

            ResultView.ItemsSource = results;
            if (results.Length == 0) {
                ResultView.Visibility = Visibility.Collapsed;
                NoResults.Visibility = Visibility.Visible;
            } else {
                ResultView.Visibility = Visibility.Visible;
                NoResults.Visibility = Visibility.Collapsed;
            }
        }

        public string SearchQuery {
            get { return (string)GetValue(SearchQueryProperty); }
            set { SetValue(SearchQueryProperty, value); }
        }

        public static readonly DependencyProperty SearchTypeProperty =
            DependencyProperty.Register("SearchType", typeof(SearchMode), typeof(SearchResults),
            new PropertyMetadata(SearchMode.User, SearchTypeChanged));

        private static async void SearchTypeChanged(DependencyObject depObject, DependencyPropertyChangedEventArgs args) {
            var control = (SearchResults)depObject;

            switch ((SearchMode)args.NewValue) {
                case SearchMode.Track:
                    control.ResultView.ItemsSource = new Track[0];
                    control.ResultView.ItemTemplate = control.TrackTemplate;
                    await control.SearchTracks(control.SearchQuery);
                    break;
                case SearchMode.User:
                    control.ResultView.ItemsSource = new User[0];
                    control.ResultView.ItemTemplate = control.UserTemplate;
                    await control.SearchUsers(control.SearchQuery);
                    break;
            }
        }

        public SearchMode SearchType {
            get { return (SearchMode)GetValue(SearchTypeProperty); }
            set { SetValue(SearchTypeProperty, value); }
        }

        private void SearchResultClicked(object sender, ItemClickEventArgs e) {
            if (SearchType == SearchMode.Track) {
                var nowPlaying = State.AppState.NowPlaying;
                nowPlaying.TrackList = ((Array)ResultView.ItemsSource).Cast<Track>().ToArray();
                nowPlaying.CurrentSong = Array.IndexOf(State.AppState.NowPlaying.TrackList, (Track)e.ClickedItem);
                ((Frame)Window.Current.Content).Navigate(typeof(Player));
            } else {

            }
        }
    }

    public enum SearchMode {
        Track = 0,
        User = 1
    }
}

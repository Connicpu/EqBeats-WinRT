using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EqBeats_WinRT.Models;
using Newtonsoft.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace EqBeats_WinRT.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home {
        public Home() {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            Featured.ItemsSource = await LoadTracks("http://eqbeats.org/tracks/featured/json");
            Latest.ItemsSource = await LoadTracks("http://eqbeats.org/tracks/latest/json");
            Random.ItemsSource = await LoadTracks("http://eqbeats.org/tracks/random/json");
        }

        private async Task<Track[]> LoadTracks(string endpoint) {
            var request = WebRequest.CreateHttp(endpoint);
            request.Accept = "application/json";
            using (var response = await request.GetResponseAsync())
            using (var responseStream = response.GetResponseStream())
            using (var responseReader = new StreamReader(responseStream)) {
                return await JsonConvert.DeserializeObjectAsync<Track[]>(await responseReader.ReadToEndAsync());
            }
        }

        private void ItemClick(object sender, ItemClickEventArgs e) {
            State.AppState.NowPlaying.TrackList = (Track[])((ListView)sender).ItemsSource;
            State.AppState.NowPlaying.PlayingTrack = (Track)e.ClickedItem;
            ((Frame)Window.Current.Content).Navigate(typeof(Player));
        }
    }
}

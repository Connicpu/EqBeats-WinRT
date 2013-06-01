using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EqBeats_WinRT.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace EqBeats_WinRT.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Player {
        private State.NowPlayingState nowPlaying;

        public Player() {
            InitializeComponent();

        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            nowPlaying = State.AppState.NowPlaying;
            if (MediaPlayer.CurrentState == MediaElementState.Stopped ||
                MediaPlayer.CurrentState == MediaElementState.Closed) {
                SetTrack(nowPlaying.TrackList[nowPlaying.CurrentSong]);
            }
        }

        public void SetTrack(Track track) {
            MediaPlayer.Source = track.Download.Mp3;
            MediaPlayer.PosterSource = new BitmapImage(track.Download.Art);
            //MediaControl.AlbumArt = track.Download.Art;
            MediaControl.ArtistName = track.Artist.Name;
            MediaControl.TrackName = track.Title;

            MediaPlayer.Play();
        }

        private void PlayerStateChanged(object sender, RoutedEventArgs e) {
            switch (MediaPlayer.CurrentState) {
                case MediaElementState.Buffering:
                    MediaControl.IsPlaying = true;
                    break;
                case MediaElementState.Closed:
                    MediaControl.IsPlaying = false;
                    break;
                case MediaElementState.Opening:
                    MediaControl.IsPlaying = true;
                    break;
                case MediaElementState.Paused:
                    MediaControl.IsPlaying = false;
                    break;
                case MediaElementState.Playing:
                    MediaControl.IsPlaying = true;
                    break;
                case MediaElementState.Stopped:
                    MediaControl.IsPlaying = false;
                    break;
            }
        }
    }
}

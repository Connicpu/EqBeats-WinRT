using System;
using System.Text.RegularExpressions;
using EqBeats_WinRT.Common;
using EqBeats_WinRT.Models;
using Windows.Media;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace EqBeats_WinRT.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Player {
        private readonly State.NowPlayingState nowPlaying;

        public Player() {
            nowPlaying = State.AppState.NowPlaying;
            InitializeComponent();
            DataContext = State.AppState;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            MediaPlayer.CurrentStateChanged += PlayerStateChanged;
            if (MediaPlayer.CurrentState == MediaElementState.Stopped ||
                MediaPlayer.CurrentState == MediaElementState.Closed) {
                SetTrack(nowPlaying.TrackList[nowPlaying.CurrentSong]);
            } else {
                UpdateScreenInfo(nowPlaying.TrackList[nowPlaying.CurrentSong]);
            }

            nowPlaying.NextSongReady += UpdateScreenInfo;
        }

        private void PlayerStateChanged(object sender, RoutedEventArgs routedEventArgs) {
            switch (MediaPlayer.CurrentState) {
                case MediaElementState.Playing:
                    PlayPauseButton.IsEnabled = true;
                    PlayPauseButton.Content = "\uE103";
                    break;
                case MediaElementState.Paused:
                case MediaElementState.Stopped:
                    PlayPauseButton.IsEnabled = true;
                    PlayPauseButton.Content = "\uE102";
                    break;
                default:
                    PlayPauseButton.IsEnabled = false;
                    break;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            nowPlaying.NextSongReady -= UpdateScreenInfo;
            MediaPlayer.CurrentStateChanged -= PlayerStateChanged;
        }

        public void SetTrack(Track track) {
            MediaPlayer.Source = track.Download.Mp3;
            MediaPlayer.Play();

            UpdateScreenInfo(track);
        }

        static readonly Regex NonAsciiRegex = new Regex(@"[^\x00-\xFF]+");
        public void UpdateScreenInfo(Track track) {
            TopPane.DataContext = track;

            var art = track.Download.Art;

            if (art != null) {
                AlbumArt.Source = new BitmapImage(art);
                AlbumArt.Visibility = Visibility.Visible;
            } else {
                AlbumArt.Visibility = Visibility.Collapsed;
            }

            MediaControl.ArtistName = track.Artist.Name;
            MediaControl.TrackName = NonAsciiRegex.Replace(track.Title, "");
        }

        private void Play() {
            MediaPlayer.Play();
            MediaControl.IsPlaying = true;
        }

        public void Stop() {
            MediaPlayer.Stop();
            MediaControl.IsPlaying = false;
        }

        public void Pause() {
            MediaPlayer.Pause();
            MediaControl.IsPlaying = false;
        }

        public MediaElement MediaPlayer {
            get {
                var player = Window.Current.FindMediaElement();

                if (!State.MediaControlInitialized) {
                    MediaControl.PlayPressed += (o, a) => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Play);
                    MediaControl.StopPressed += (o, a) => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Stop);
                    MediaControl.PausePressed += (o, a) => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Pause);
                    MediaControl.NextTrackPressed += (o, a) =>
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, nowPlaying.NextTrack);
                    MediaControl.PreviousTrackPressed += (o, a) =>
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, nowPlaying.PreviousTrack);
                    MediaControl.PlayPauseTogglePressed += (o, a) => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate {
                        switch (player.CurrentState) {
                            case MediaElementState.Playing:
                                Pause();
                                break;
                            case MediaElementState.Paused:
                                Play();
                                break;
                        }
                    });

                    player.MediaEnded += delegate { nowPlaying.NextTrack(); };

                    var positionBinding = new Binding {
                        Source = nowPlaying,
                        Path = new PropertyPath("SongPosition"),
                        Mode = BindingMode.TwoWay
                    };
                    player.SetBinding(MediaElement.PositionProperty, positionBinding);

                    State.PlayerUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
                    State.PlayerUpdateTimer.Tick += delegate {
                        nowPlaying.TrackDuration = player.NaturalDuration;
                    };
                    State.PlayerUpdateTimer.Start();

                    State.MediaControlInitialized = true;
                }

                return player;
            }
        }

        private void GoBack(object sender, RoutedEventArgs e) {
            ((Frame)Window.Current.Content).Navigate(typeof(Home));
        }

        private void PlaylistItemChanged(object sender, SelectionChangedEventArgs e) {
            ((ListView)sender).ScrollIntoView(nowPlaying.PlayingTrack);
        }

        private void PreviousButtonClick(object sender, RoutedEventArgs e) {
            nowPlaying.PreviousTrack();
        }

        private void PlayPauseButtonClick(object sender, RoutedEventArgs e) {
            if (MediaControl.IsPlaying) {
                Pause();
            } else {
                Play();
            }
        }

        private void NextButtonClick(object sender, RoutedEventArgs e) {
            nowPlaying.NextTrack();
        }
    }
}

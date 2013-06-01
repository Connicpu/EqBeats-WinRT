using System.Threading.Tasks;
using EqBeats_WinRT.Models;
using EqBeats_WinRT.Pages;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace EqBeats_WinRT {
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App() {
            InitializeComponent();
            Suspending += OnSuspending;

            RequestedTheme = ApplicationTheme.Light;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args) {
            await EnsureMainPageActivated(args);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e) {
            var deferral = e.SuspendingOperation.GetDeferral();
            await State.SaveState();
            deferral.Complete();
        }

        private static async Task EnsureMainPageActivated(IActivatedEventArgs e) {
            if (e.PreviousExecutionState == ApplicationExecutionState.Terminated || !State.HasState) {
                await State.LoadState();
            }
            if (Window.Current.Content == null) {
                var rootFrame = new Frame();
                rootFrame.Navigated += (sender, args) => {
                    State.AppState.CurrentPageType = args.SourcePageType;
                };
                rootFrame.Navigate(State.AppState.CurrentPageType);
                Window.Current.Content = rootFrame;
            }
            Window.Current.Activate();
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args) {
            SearchPane.GetForCurrentView().QuerySubmitted += (sender, eventArgs) => SubmitSearch(eventArgs.QueryText);
            SearchPane.GetForCurrentView().QueryChanged += (sender, eventArgs) => UpdateSearch(eventArgs.QueryText);
        }

        protected override async void OnSearchActivated(SearchActivatedEventArgs args) {
            await EnsureMainPageActivated(args);
            UpdateSearch(args.QueryText);
            SubmitSearch(args.QueryText);
        }

        private static void SubmitSearch(string text) {
            var frame = Window.Current.Content as Frame;
            if (State.AppState == null || string.IsNullOrWhiteSpace(text) || frame == null) return;

            if (frame.CurrentSourcePageType != typeof(Search)) {
                frame.Navigate(typeof(Search));
            }

            State.AppState.SearchQuery.SubmittedQuery = text;
        }


        private static void UpdateSearch(string text) {
            var frame = Window.Current.Content as Frame;
            if (State.AppState == null || frame == null) return;

            if (frame.CurrentSourcePageType != typeof(Search)) {
                frame.Navigate(typeof(Search));
            }

            State.AppState.SearchQuery.CurrentQuery = text;
        }
    }
}

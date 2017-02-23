using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Business;
using DataAccess;
using System.Threading;
using System.Windows.Controls;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Holds all the components required to manage a session.
    /// </summary>
    public class SessionCore {
        public static SessionCore Instance { get; private set; }
        public PlayerBusiness Business { get; private set; }
        public MenuControl Menu { get; private set; }
        public LayersControl Layers { get; private set; }
        public RatingViewerControl RatingViewer { get; private set; }
        public ViewDownloadsWindow Downloads { get; private set; }
        public ImageSource Icon { get; private set; }
        public Window Main { get; private set; }
        public Bitmap SplashImage { get; set; }
        public MainWindow NgMain {
            get {
                return Main as MainWindow;
            }
        }
        public WindowManager Windows { get; private set; }

        static SessionCore() {
            Instance = new SessionCore();
        }

        public void Start(Window main, System.Drawing.Bitmap splashImage) {
            // Make sure to initialize Settings static constructor to initialize database path.
            var a = Settings.SavedFile;

            NaturalGroundingPlayer.MainWindow NgMain = main as NaturalGroundingPlayer.MainWindow;
            if (NgMain != null) {
                Menu = (MenuControl)NgMain.MainMenuContainer.Content;
                Layers = (LayersControl)NgMain.LayersContainer.Content;
                RatingViewer = (RatingViewerControl)NgMain.RatingViewerContainer.Content;
                Business = new PlayerBusiness(GetNewPlayer());
            }
            Icon = main.Icon;
            Main = main;
            SplashImage = splashImage;

            main.Loaded += Main_Loaded;
            main.Closing += Main_Closing;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            main.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            System.Windows.Forms.Application.ThreadException += Application_ThreadException; // MPC-HC API calls are done through WinForms.

            Windows = new WindowManager(main);
        }

        public IMediaPlayerBusiness GetNewPlayer() {
            if (Settings.SavedFile == null)
                return null;
            else if (Settings.SavedFile.MediaPlayerApp == MediaPlayerApplication.Mpc)
                return new MpcPlayerBusiness();
            else
                return new WmpPlayerBusiness(WmpPlayerWindow.Instance().Player);
        }

        private async void Main_Loaded(object sender, EventArgs e) {
            if (NgMain != null) {
                Downloads = new ViewDownloadsWindow();
                Downloads.Owner = Main;
                Downloads.DownloadsView.ItemsSource = Business.DownloadManager.DownloadsList;
            }

            SplashWindow InitWin = SplashWindow.Instance(Main, SplashImage);
            SplashImage = null;
            // InitWin.Owner = Main;

            CancellationToken token = new CancellationToken();
            Task InitWinTask = Task.Factory.StartNew(
                            () => InitWin.ShowDialog(),
                            token,
                            TaskCreationOptions.None,
                            TaskScheduler.FromCurrentSynchronizationContext());

            await TestDatabaseConnectionAsync();

            if (Settings.SavedFile == null) {
                AboutWindow.Instance();
                SettingsWindow.Instance();
            }

            if (NgMain != null)
                await NgMain.InitializationCompleted();

            InitWin.CanClose();
            await InitWinTask;
        }

        /// <summary>
        /// Close the video player and all windows.
        /// </summary>
        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Business != null) {
                RatingViewer.UpdatePreference();
                Business.ClosePlayer();
                Downloads.Close();
                Application.Current.Shutdown();
            }
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            HandleFatalException(e.Exception);
            e.Handled = true;
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e) {
            HandleFatalException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Exception ExObj = e.ExceptionObject as Exception;
            if (ExObj != null)
                HandleFatalException(ExObj);
            else
                HandleFatalException(new Exception(e.ExceptionObject.ToString()));
        }

        private void HandleFatalException(Exception e) {
            string ErrorMessage = "An unhandled exception has occured. Do you want to see the error details? You may send this report by email to etienne@spiritualselftransformation.com";
            ErrorMessage += Environment.NewLine + Environment.NewLine + e.Message;
            if (MessageBox.Show(ErrorMessage, "Unhandled Exception", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                DatabaseHandler Handler = new DatabaseHandler(Main);
                Handler.LogException(e);
            }
        }

        private async Task TestDatabaseConnectionAsync() {
            // Test connection to database.
            DatabaseHandler db = new DatabaseHandler(Main);
            try {
                await db.EnsureAvailableAsync();
            }
            catch (Exception ex) {
                string ErrorMessage = "Cannot connect to database.\r\n" + ex.Message;
                MessageBox.Show(Main, ErrorMessage, "Natural Grounding Player");
                Application.Current.Shutdown();
            }
        }

        public static string GetVersionText() {
            Version CurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            StringBuilder Title = new StringBuilder();
            Title.Append(CurrentVersion.Major);
            Title.Append('.');
            Title.Append(CurrentVersion.Minor);
            if (CurrentVersion.Build > 0) {
                Title.Append('.');
                Title.Append(CurrentVersion.Build);
                if (CurrentVersion.Revision > 0) {
                    Title.Append('.');
                    Title.Append(CurrentVersion.Revision);
                }
            }
            return Title.ToString();
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.Generic;
using Business;
using DataAccess;
using YoutubeExtractor;
using Microsoft.Win32;
using EmergenceGuardian.FFmpeg;

namespace NaturalGroundingPlayer {
    public delegate void ClosingCallback(Media result);

    /// <summary>
    /// Interaction logic for EditVideoWindow.xaml
    /// </summary>
    public partial class EditVideoWindow : Window {
        /// <summary>
        /// Displays a window to edit specified video.
        /// </summary>
        public static EditVideoWindow Instance(Guid? videoId, string fileName, ClosingCallback callback) {
            EditVideoWindow NewForm = new EditVideoWindow();
            if (videoId != null && videoId != Guid.Empty)
                NewForm.videoId = videoId;
            else
                NewForm.fileName = fileName;
            NewForm.callback = callback;
            SessionCore.Instance.Windows.Show(NewForm);
            return NewForm;
        }

        /// <summary>
        /// Displays a popup containing the FileBinding menu features.
        /// </summary>
        public static EditVideoWindow InstancePopup(UIElement target, PlacementMode placement, Guid? videoId, string fileName, ClosingCallback callback) {
            EditVideoWindow NewForm = new EditVideoWindow();
            NewForm.isPopup = true;
            NewForm.videoId = videoId;
            if (videoId != null && videoId != Guid.Empty)
                NewForm.videoId = videoId;
            else
                NewForm.fileName = fileName;
            NewForm.callback = callback;
            WindowHelper.SetScale(NewForm.FileBindingButton.ContextMenu);
            NewForm.Window_Loaded(null, null);
            NewForm.ShowFileBindingMenu(target, placement);
            return NewForm;
        }

        protected Guid? videoId;
        protected string fileName;
        protected ClosingCallback callback;
        protected bool isFormSaved = false;
        private EditVideoBusiness business = new EditVideoBusiness();
        private EditRatingsBusiness ratingBusiness;
        private Media video;
        private bool fileNotFound;
        private bool downloaded;
        private bool isNew;
        private bool isUrlValid;
        private bool isPopup;
        private WindowHelper helper;

        public EditVideoWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            if (videoId != null) {
                video = business.GetVideoById(videoId.Value);
                if (video.MediaId == Guid.Empty)
                    throw new InvalidDataException("This Media information has an invalid empty GUID.");
            } else {
                video = business.GetVideoByFileName(fileName);
                if (video == null) {
                    video = business.NewVideo();
                    video.FileName = fileName;
                    video.MediaTypeId = (int)EditVideoBusiness.GetFileType(fileName);
                    video.DownloadName = System.IO.Path.GetFileNameWithoutExtension(fileName);
                    isNew = true;
                }
            }

            this.DataContext = video;
            CategoryCombo.ItemsSource = business.GetCategories(video.MediaTypeId);
            Custom1Combo.ItemsSource = business.GetCustomRatingCategories();
            Custom2Combo.ItemsSource = Custom1Combo.ItemsSource;
            ratingBusiness = business.GetRatings(video);
            RatingsGrid.DataContext = ratingBusiness;
            EditRating_LostFocus(null, null);

            if (video.FileName != null) {
                if (File.Exists(Settings.NaturalGroundingFolder + video.FileName))
                    await LoadMediaInfoAsync();
                else {
                    // Try to auto-attach same path with different extension.
                    if (!EditPlaylistBusiness.AutoAttachFile(video, PathManager.GetPathWithoutExtension(video.FileName))) {
                        fileNotFound = true;
                        ErrorText.Text = "File not found.";
                    }
                }
            }
        }

        private async Task LoadMediaInfoAsync() {
            FFmpegProcess FileInfo = await Task.Run(() => MediaInfo.GetFileInfo(Settings.NaturalGroundingFolder + video.FileName));
            DimensionText.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            DisablePitchCheckBox.IsEnabled = FileInfo?.VideoStream?.PixelAspectRatio == 1;
            if (!DisablePitchCheckBox.IsEnabled)
                video.DisablePitch = false;
        }

        private async void DownloadUrlText_LostFocus(object sender, RoutedEventArgs e) {
            isUrlValid = false;
            ErrorText.Text = "";
            if (video.DownloadUrl.Length > 0) {
                try {
                    var VTask = DownloadBusiness.GetDownloadUrlsAsync(video.DownloadUrl);
                    var VideoList = await VTask;
                    VideoInfo FirstVid = VideoList.FirstOrDefault();
                    if (FirstVid != null) {
                        video.DownloadName = FirstVid.Title;
                        isUrlValid = true;
                    }
                } catch { }
                if (!isUrlValid)
                    ErrorText.Text = "Please enter a valid URL";
            }
        }

        private void YouTubeSearchButton_Click(object sender, RoutedEventArgs e) {
            if (DownloadNameText.Text.Length > 0) {
                string SearchQuery = Uri.EscapeDataString(DownloadNameText.Text);
                Process.Start("https://www.youtube.com/results?search_query=" + SearchQuery);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            if (SaveChanges())
                this.Close();
        }

        private bool SaveChanges() {
            SaveButton.Focus();
            video.Artist = video.Artist.Trim();
            video.Album = video.Album.Trim();
            video.Title = video.Title.Trim();
            video.DownloadName = video.DownloadName.Trim();
            video.DownloadUrl = video.DownloadUrl.Trim();
            video.BuyUrl = video.BuyUrl.Trim();

            ErrorText.Text = "";
            if (TitleText.Text.Length == 0) {
                ErrorText.Text = "Title is required.";
                return false;
            }
            if (business.IsTitleDuplicate(video)) {
                ErrorText.Text = "Artist and title already exist in the database.";
                return false;
            }

            // Only update EditedOn when directly editing from the Edit window.
            if (!isPopup)
                video.EditedOn = DateTime.UtcNow;

            ratingBusiness.UpdateChanges();
            business.Save();
            isFormSaved = true;

            // Update grid when in popup mode. Otherwise Callback is called in Window_Closing.
            if (isPopup)
                callback(video);

            return true;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e) {
            if (video.FileName != null) {
                await SessionCore.Instance.Business.PlaySingleVideoAsync(video.FileName);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            //if (editMode != EditVideoWindowMode.Popup)
            //    Owner.Show();
            if (callback != null) {
                if (isFormSaved)
                    callback(video);
                else
                    callback(null);
            }
        }

        #region File Binding Menu

        private void FileBindingButton_Click(object sender, RoutedEventArgs e) {
            ShowFileBindingMenu(FileBindingButton, PlacementMode.Bottom);
        }

        public void ShowFileBindingMenu(UIElement target, PlacementMode placement) {
            // Set context menu items visibility.
            menuPlay.IsEnabled = (!fileNotFound && video.FileName != null);
            if (!isPopup)
                FileBindingButton.ContextMenu.Items.Remove(menuEdit);
            string DefaultFileName = GetDefaultFileName();
            menuMoveFile.IsEnabled = (video.Title.Length > 0 && video.FileName != null && video.FileName != DefaultFileName);
            if (menuMoveFile.IsEnabled)
                menuMoveFile.Header = string.Format("_Move to \"{0}\"", DefaultFileName);
            else
                menuMoveFile.Header = "_Move to Default Location";
            if (isNew)
                menuSelectFile.Header = "_Select Existing Entry...";
            else
                menuSelectFile.Header = "_Select Another File...";
            menuDownloadVideo.IsEnabled = (!downloaded && (fileNotFound || video.FileName == null) && video.DownloadUrl.Length > 0);
            menuExtractAudio.IsEnabled = (!fileNotFound && video.FileName != null);
            menuRemoveBinding.IsEnabled = (!isNew && video.FileName != null);
            menuDeleteVideo.IsEnabled = (!fileNotFound && video.FileName != null);
            if (isNew)
                menuDeleteVideo.Header = "Delete _File";
            else
                menuDeleteVideo.Header = "Delete Attached _File";
            menuDeleteEntry.IsEnabled = !isNew;

            // Show context menu.
            FileBindingButton.ContextMenu.IsEnabled = true;
            FileBindingButton.ContextMenu.PlacementTarget = target;
            FileBindingButton.ContextMenu.Placement = placement;
            FileBindingButton.ContextMenu.IsOpen = true;
        }

        private void menuEdit_Click(object sender, RoutedEventArgs e) {
            EditVideoWindow.Instance(null, video.FileName, callback);
        }

        private void menuMoveFile_Click(object sender, RoutedEventArgs e) {
            MoveFilesBusiness MoveBusiness = new MoveFilesBusiness();
            string DefaultFileName = GetDefaultFileName();
            if (MoveBusiness.MoveFile(video, DefaultFileName)) {
                video.FileName = DefaultFileName;
                SaveChanges();
                fileNotFound = false;
            }
        }

        private string GetDefaultFileName() {
            DefaultMediaPath PathCalc = new DefaultMediaPath();
            string Result = PathCalc.GetDefaultFileName(video.Artist, video.Title, video.MediaCategoryId, (MediaType)video.MediaTypeId) + Path.GetExtension(video.FileName);
            return Result;
        }

        private async void menuSelectFile_Click(object sender, RoutedEventArgs e) {
            if (!isNew) {
                // Bind to another file.
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.InitialDirectory = Settings.NaturalGroundingFolder;
                if (video.MediaType == MediaType.Video)
                    dlg.Filter = string.Format("Video Files|*{0})", string.Join(";*", Settings.VideoExtensions));
                else if (video.MediaType == MediaType.Audio)
                    dlg.Filter = string.Format("Audio Files|*{0})", string.Join(";*", Settings.AudioExtensions));
                else if (video.MediaType == MediaType.Image)
                    dlg.Filter = string.Format("Image Files|*{0})", string.Join(";*", Settings.ImageExtensions));
                if (dlg.ShowDialog(IsLoaded ? this : Owner).Value == true) {
                    if (!dlg.FileName.StartsWith(Settings.NaturalGroundingFolder))
                        MessageBox.Show("You must select a file within your Natural Grounding folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    else {
                        string BindFile = dlg.FileName.Substring(Settings.NaturalGroundingFolder.Length);
                        if (business.GetVideoByFileName(BindFile) == null) {
                            video.FileName = BindFile;
                            video.Length = null;
                            video.Height = null;
                            await LoadMediaInfoAsync();
                            if (isPopup)
                                SaveChanges();
                        } else
                            MessageBox.Show("This file is already in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    fileNotFound = false;
                }
            } else {
                // Bind file to an existing entry.
                SearchSettings settings = new SearchSettings() {
                    MediaType = (MediaType)video.MediaTypeId,
                    ConditionField = FieldConditionEnum.FileExists,
                    ConditionValue = BoolConditionEnum.No,
                    ListIsInDatabase = true
                };
                VideoListItem Result = SearchVideoWindow.Instance(settings);
                if (Result != null) {
                    // Close and re-open selected entry.
                    Close();
                    EditVideoWindow NewForm = Instance(Result.MediaId, null, callback);
                    NewForm.video.FileName = video.FileName;
                    NewForm.video.Length = null;
                    NewForm.video.Height = null;
                    await NewForm.LoadMediaInfoAsync();
                }
            }
        }

        private async void menuDownloadVideo_Click(object sender, RoutedEventArgs e) {
            await menuDownloadVideo_ClickAsync();
        }

        private async Task menuDownloadVideo_ClickAsync() {
            await SessionCore.Instance.Business.DownloadManager.DownloadVideoAsync(video, -1, null);
            downloaded = true;
        }

        private async void menuExtractAudio_Click(object sender, RoutedEventArgs e) {
            if (video.FileName != null && File.Exists(Settings.NaturalGroundingFolder + video.FileName)) {
                FFmpegProcess MInfo = await Task.Run(() => MediaInfo.GetFileInfo(Settings.NaturalGroundingFolder + video.FileName));
                string Ext = null;
                if (MInfo?.AudioStream?.Format == "MPEG Audio")
                    Ext = ".mp2";
                else if (MInfo?.AudioStream?.Format == "PCM")
                    Ext = ".wav";
                else if (MInfo?.AudioStream?.Format == "Vorbis")
                    Ext = ".ogg";
                else if (MInfo?.AudioStream?.Format == "Opus")
                    Ext = ".opus";
                else
                    Ext = ".aac";

                SaveFileDialog SaveDlg = new SaveFileDialog();
                SaveDlg.InitialDirectory = Settings.NaturalGroundingFolder + "Audios";
                SaveDlg.OverwritePrompt = true;
                SaveDlg.DefaultExt = ".mp3";
                SaveDlg.Filter = string.Format("Audio Files|*{0})", Ext); ;
                SaveDlg.FileName = Path.GetFileNameWithoutExtension(video.FileName) + Ext;

                if (SaveDlg.ShowDialog() == true) {
                    MediaMuxer.ExtractAudio(Settings.NaturalGroundingFolder + video.FileName, SaveDlg.FileName);
                }
            }
        }

        private void menuRemoveBinding_Click(object sender, RoutedEventArgs e) {
            video.FileName = null;
            video.Length = null;
            video.Height = null;
            if (isPopup)
                SaveChanges();
        }

        private void menuDeleteVideo_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Are you sure you want to delete this video file?" + Environment.NewLine + video.FileName, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                try {
                    business.DeleteFile(video.FileName);
                    video.FileName = null;
                    video.Length = null;
                    video.Height = null;
                    bool IsSaved = SaveChanges();
                    // If adding new record and deleting the file and can't save, we must close form
                    // to avoid duplicate record if downloading another video file.
                    if (isNew && !IsSaved) {
                        video.MediaId = Guid.Empty;
                        isFormSaved = true;
                        this.Close(); // A non-loaded window can still be closed...
                    }
                } catch (Exception ex) {
                    string Msg = "The file cannot be deleted:\r\n" + ex.Message;
                    MessageBox.Show(Msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void menuDeleteEntry_Click(object sender, RoutedEventArgs e) {
            string Title;
            if (video.Artist.Length > 0)
                Title = string.Format("{0} - {1}", video.Artist, video.Title);
            else
                Title = video.Title;
            if (MessageBox.Show("Are you sure you want to delete this database entry?\r\nThe file will not be deleted.\r\n" + Title, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                business.Delete(video);
                isFormSaved = true;
                this.Close();
            }
        }

        #endregion

        private void CustomCombo_LostFocus(object sender, KeyboardFocusChangedEventArgs e) {
            ComboBox Obj = sender as ComboBox;
            RatingCategory ObjItem = Obj.SelectedItem as RatingCategory;
            if (ObjItem != null)
                Obj.Text = ObjItem.Name;
            else
                Obj.Text = "";

            if (Obj.Text.Length > 0 && Custom1Combo.Text == Custom2Combo.Text)
                Obj.Text = "";
        }

        private void CategoryCombo_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            ComboBox Obj = sender as ComboBox;
            MediaCategory ObjItem = Obj.SelectedItem as MediaCategory;
            if (ObjItem != null)
                Obj.Text = ObjItem.Name;
            else
                Obj.Text = "";
        }

        private void EditRating_LostFocus(object sender, RoutedEventArgs e) {
            if (sender != null)
                (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
            RatingViewerControl.DisplayValue(PMValueText, ratingBusiness.PM, 0);
            RatingViewerControl.DisplayValue(PFValueText, ratingBusiness.PF, 0);
            RatingViewerControl.DisplayValue(EMValueText, ratingBusiness.EM, 0);
            RatingViewerControl.DisplayValue(EFValueText, ratingBusiness.EF, 0);
            RatingViewerControl.DisplayValue(SMValueText, ratingBusiness.SM, 0);
            RatingViewerControl.DisplayValue(SFValueText, ratingBusiness.SF, 0);
            RatingViewerControl.DisplayValue(LoveValueText, ratingBusiness.Love, 0);
            RatingViewerControl.DisplayValue(EgolessValueText, ratingBusiness.Egoless, 0);
            RatingViewerControl.DisplayValue(Custom1ValueText, ratingBusiness.Custom1, 0);
            RatingViewerControl.DisplayValue(Custom2ValueText, ratingBusiness.Custom2, 0);
        }
    }
}

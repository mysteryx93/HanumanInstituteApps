using System;
using System.Globalization;
using System.Net;
using AngleSharp;
using AngleSharp.Dom;
// using AngleSharp.Parser.Html;

namespace EmergenceGuardian.MpcWebInterface {
    /// <summary>
    /// Exposes MPC-HC's Web Interface API.
    /// </summary>
    public class MpcApi {

        #region Declarations / Constructor

        /// <summary>
        /// The URI of the server to connect to. Default is localhost.
        /// </summary>
        public string Server { get; set; } = "localhost";
        /// <summary>
        /// The Port on the server to connect to. Default is 13579.
        /// </summary>
        public int Port { get; set; } = 13579;

        /// <summary>
        /// Initializes a new instance of the MpcApi class.
        /// </summary>
        public MpcApi() { }

        /// <summary>
        /// Initializes a new instance of the MpcApi class using specified port for connection on localhost.
        /// </summary>
        /// <param name="port">The port to connect to.</param>
        public MpcApi(int port) {
            Port = port;
        }

        /// <summary>
        /// Initializes a new instance of the MpcApi class using specified uri and port for connection.
        /// </summary>
        /// <param name="server">The uri to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        public MpcApi(string server, int port) {
            Server = server;
            Port = port;
        }

        #endregion

        #region Queries

        /// <summary>
        /// Queries MPC-HC variables data.
        /// </summary>
        /// <returns>An object containing all variables data</returns>
        public MpcVariables GetVariables() {
            // Query HTML data.
            string Page = QueryPage("variables.html");

            // Parse HTML.
            var Parser = new HtmlParser();
            var Doc = Parser.Parse(Page);
            var Elements = Doc.QuerySelectorAll("p");

            // Parse Elements.
            MpcVariables Result = new MpcVariables();
            Result.File = Elements["file"].TextContent;
            Result.FilePathArg = Elements["filepatharg"].TextContent;
            Result.FilePath = Elements["filepath"].TextContent;
            Result.FileDirArg = Elements["filedirarg"].TextContent;
            Result.FileDir = Elements["filedir"].TextContent;
            Result.State = (MpcState)int.Parse(Elements["state"].TextContent);
            Result.StateString = Elements["statestring"].TextContent;
            Result.Position = TimeSpan.FromMilliseconds(int.Parse(Elements["position"].TextContent));
            Result.PositionString = Elements["positionstring"].TextContent;
            Result.Duration = TimeSpan.FromMilliseconds(int.Parse(Elements["duration"].TextContent));
            Result.DurationString = Elements["durationstring"].TextContent;
            Result.VolumeLevel = int.Parse(Elements["volumelevel"].TextContent);
            Result.Muted = Elements["muted"].TextContent != "0";
            Result.PlaybackRate = float.Parse(Elements["playbackrate"].TextContent);
            Result.Size = Elements["size"].TextContent;
            Result.ReloadTime = int.Parse(Elements["reloadtime"].TextContent);
            Result.Version = Version.Parse(Elements["version"].TextContent);
            return Result;
        }

        public void OpenFile(string path) => QueryPage(string.Format("browser.html?path={0}", Uri.EscapeDataString(path)));

        public void Seek(TimeSpan position) => PushCommand(-1, "position", position.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture));
        public void Seek(float percent) => PushCommand(-1, "percent", (percent * 100).ToString(CultureInfo.InvariantCulture));
        public void Play() => PushCommand(887);
        public void Pause() => PushCommand(888);
        public void Stop() => PushCommand(890);
        public void SkipBack() => PushCommand(921);
        public void DecreaseSpeed() => PushCommand(894);
        public void IncreaseSpeed() => PushCommand(895);
        public void SkipForward() => PushCommand(922);
        public void FramestepBack() => PushCommand(892);
        public void Framestep() => PushCommand(891);
        public void JumpBackwardKeyframe() => PushCommand(897);
        public void JumpBackwardLarge() => PushCommand(903);
        public void JumpBackwardMedium() => PushCommand(901);
        public void JumpBackwardSmall() => PushCommand(899);
        public void JumpForwardSmall() => PushCommand(900);
        public void JumpForwardMedium() => PushCommand(902);
        public void JumpForwardLarge() => PushCommand(904);
        public void JumpForwardKeyframe() => PushCommand(898);
        public void Mute() => PushCommand(909);
        public void Volume(int volume) => PushCommand(-2, "volume", volume.ToString(CultureInfo.InvariantCulture));
        public void VolumeDown() => PushCommand(908);
        public void VolumeUp() => PushCommand(907);
        public void PlaylistSkipBack() => PushCommand(919);
        public void PlaylistSkipForward() => PushCommand(920);
        public void BossKey() => PushCommand(944);
        public void AudioDelayDecrease() => PushCommand(906);
        public void AudioDelayIncrease() => PushCommand(905);
        public void PanUpLeft() => PushCommand(872);
        public void PanUp() => PushCommand(870);
        public void PanUpRight() => PushCommand(873);
        public void PanLeft() => PushCommand(868);
        public void PanCenter() => PushCommand(876);
        public void PanRight() => PushCommand(869);
        public void PanDownLeft() => PushCommand(874);
        public void PanDown() => PushCommand(871);
        public void PanDownRight() => PushCommand(875);
        public void PanIncreaseHeight() => PushCommand(866);
        public void PanIncreaseSize() => PushCommand(862);
        public void PanDecreaseWidth() => PushCommand(865);
        public void PanResetSize() => PushCommand(861);
        public void PanIncreaseWidth() => PushCommand(864);
        public void PanDecreaseSize() => PushCommand(863);
        public void PanDecreaseHeight() => PushCommand(867);
        public void VideoFrameHalf() => PushCommand(835);
        public void VideoFrameDouble() => PushCommand(837);
        public void VideoFrameNormal() => PushCommand(836);
        public void VideoFrameStretch() => PushCommand(838);
        public void VideoFrameInside() => PushCommand(839);
        public void VideoFrameOutside() => PushCommand(840);
        public void DvdMenuUp() => PushCommand(931);
        public void DvdMenuLeft() => PushCommand(929);
        public void DvdMenuActivate() => PushCommand(933);
        public void DvdMenuRight() => PushCommand(930);
        public void DvdMenuBack() => PushCommand(934);
        public void DvdMenuDown() => PushCommand(932);
        public void DvdMenuLeave() => PushCommand(935);
        public void OpenFile() => PushCommand(800);
        public void OpenDvd() => PushCommand(801);
        public void OpenDevice() => PushCommand(802);
        public void SaveCopy() => PushCommand(805);
        public void Close() => PushCommand(804);
        public void Exit() => PushCommand(816);
        public void GoTo() => PushCommand(893);
        public void Options() => PushCommand(886);
        public void Properties() => PushCommand(814);
        public void Zoom50() => PushCommand(832);
        public void Zoom100() => PushCommand(833);
        public void Zoom200() => PushCommand(834);
        public void Fullscreen() => PushCommand(830);
        public void FullscreenNoResChange() => PushCommand(831);
        public void PlayerViewMinimal() => PushCommand(827);
        public void PlayerViewCompact() => PushCommand(828);
        public void PlayerViewNormal() => PushCommand(829);
        public void ViewPlaylist() => PushCommand(824);
        public void ViewSubresync() => PushCommand(823);
        public void ViewCapture() => PushCommand(825);
        public void ViewCaptionMenu() => PushCommand(817);
        public void ViewSeeker() => PushCommand(818);
        public void ViewControls() => PushCommand(819);
        public void ViewInformation() => PushCommand(820);
        public void ViewStatistics() => PushCommand(821);
        public void ViewStatus() => PushCommand(822);
        public void PreviousGenericAudio() => PushCommand(953);
        public void NextGenericAudio() => PushCommand(952);
        public void PreviousGenericSubtitle() => PushCommand(955);
        public void NextGenericSubtitle() => PushCommand(954);
        public void PreviousOgmAudio() => PushCommand(958);
        public void NextOgmAudio() => PushCommand(957);
        public void PreviousOgmSubtitle() => PushCommand(960);
        public void NextOgmSubtitle() => PushCommand(959);
        public void LoadSubtitles() => PushCommand(809);
        public void SaveSubtitles() => PushCommand(810);
        public void PreviousDvdAngle() => PushCommand(962);
        public void NextDvdAngle() => PushCommand(961);
        public void PreviousDvdAudio() => PushCommand(964);
        public void NextDvdAudio() => PushCommand(963);
        public void PreviousDvdSubtitle() => PushCommand(966);
        public void NextDvdSubtitle() => PushCommand(965);
        public void DvdTitle() => PushCommand(923);
        public void DvdRoot() => PushCommand(924);
        public void DvdMenuSubtitle() => PushCommand(925);
        public void DvdMenuAudio() => PushCommand(926);
        public void DvdMenuAngle() => PushCommand(927);
        public void DvdMenuChapter() => PushCommand(928);
        public void MenuFilters() => PushCommand(951);
        public void MenuPlayerShort() => PushCommand(949);
        public void MenuPlayerLong() => PushCommand(950);
        public void AlwaysOnTopAlways() => PushCommand(884);
        public void AlwaysOnTopWhilePlaying() => PushCommand(885);
        public void AlwaysOnTopWhilePlayingVideo() => PushCommand(886);
        public void AlwaysOnTopNever() => PushCommand(883);

        /// <summary>
        /// Sends a command to MPC-HC.
        /// </summary>
        /// <param name="commandId">The ID of the command to send.</param>
        public void PushCommand(int commandId) {
            PushCommand(commandId, null, null);
        }

        /// <summary>
        /// Sends a command to MPC-HC.
        /// </summary>
        /// <param name="commandId">The ID of the command to send.</param>
        /// <param name="argName">The name of an additional argument, if applicable.</param>
        /// <param name="argValue">The value of an additional argument, if applicable.</param>
        public void PushCommand(int commandId, string argName, string argValue) {
            string Request = string.Format("command.html?wm_command={0}{1}", commandId, !string.IsNullOrEmpty(argName) ? string.Format("&{0}={1}", argName, argValue) : "");
            QueryPage(Request);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Queries specified page and returns the content.
        /// </summary>
        /// <param name="uri">The relative Uri to request.</param>
        /// <returns>The text content of the page.</returns>
        private string QueryPage(string uri) {
            using (var client = new WebClient()) {
                return client.DownloadString(GetRequestUri(uri));
            }
        }

        /// <summary>
        /// Returns a full URI that can be queried.
        /// </summary>
        /// <param name="uri">The page to request on the server.</param>
        /// <returns>The full request URI.</returns>
        private string GetRequestUri(string uri) {
            return string.Format("http://{0}:{1}/{2}", Server, Port, uri);
        }

        #endregion

    }
}

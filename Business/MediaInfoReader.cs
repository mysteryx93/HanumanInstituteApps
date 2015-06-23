using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using MediaPlayer;

namespace Business {
    /// <summary>
    /// Provides access to media information that is only available by opening up the file.
    /// </summary>
    public class MediaInfoReader : IDisposable {
        // private WindowsMediaPlayer viewer = new WindowsMediaPlayer();
        MediaInfo lastMedia;
        // TaskCompletionSource<bool> openFileTask = new TaskCompletionSource<bool>();

        /// <summary>
        /// Initializes a new instance of the MediaInfoReader class.
        /// </summary>
        public MediaInfoReader() {
            //viewer.Volume = 0;
            //viewer.MediaOpened += (sender, e) => {
            //    openFileTask.TrySetResult(true);
            //};
        }

        /// <summary>
        /// Loads the Length, Width and Height of specified media file.
        /// </summary>
        /// <param name="item">The media file to read.</param>
        /// <returns>True if data was loaded, false if no data was needed.</returns>
        public async Task<bool> LoadInfoAsync(Media item) {
            bool HasChanges = false;
            if (HasMissingInfo(item)) {
                await LoadInfoAsync(Settings.NaturalGroundingFolder + item.FileName);
                item.Length = Length;
                item.Height = Height;
                HasChanges = true;
            }
            return HasChanges;
        }

        /// <summary>
        /// Loads media information of specified file.
        /// </summary>
        /// <param name="fileName">The full path of the file to read.</param>
        public async Task LoadInfoAsync(string fileName) {
            lastMedia = await Task.Run(() => ReadMediaFile(fileName));

            //// Open file and wait for task to complete with a timeout of 3 seconds.
            //openFileTask = new TaskCompletionSource<bool>();
            //viewer.Source = Settings.NaturalGroundingFolder + item.FileName;
            //if (await Task.WhenAny(openFileTask.Task, Task.Delay(3000)) == openFileTask.Task) {
            //    viewer.Stop();
            //    if (openFileTask.Task.Result == true) {
            //        // Media file opened.
            //        if (viewer.Duration > 0)
            //            item.Length = (short)viewer.Duration;
            //        if (HasDimensions(item)) {
            //            item.Width = (short)viewer.VideoWidth;
            //            item.Height = (short)viewer.VideoHeight;
            //        } else {
            //            item.Width = null;
            //            item.Height = null;
            //        }
            //        HasChanges = true;
            //    }
            //} else
            //    openFileTask.TrySetCanceled();
        }

        public short? Length {
            get {
                string StrValue = lastMedia.Get(StreamKind.Video, 0, "Duration");
                int Result = 0;
                if (int.TryParse(StrValue, out Result))
                    return (short)(Result / 1000);
                else
                    return null;
            }
        }

        public short? Width {
            get {
                string StrValue = lastMedia.Get(StreamKind.Video, 0, "Width");
                short Result = 0;
                if (short.TryParse(StrValue, out Result))
                    return Result;
                else
                    return null;
            }
        }

        public short? Height {
            get {
                string StrValue = lastMedia.Get(StreamKind.Video, 0, "Height");
                short Result = 0;
                if (short.TryParse(StrValue, out Result))
                    return Result;
                else
                    return null;
            }
        }

        public double? PixelAspectRatio {
            get {
                string StrValue = lastMedia.Get(StreamKind.Video, 0, "PixelAspectRatio");
                double Result = 0;
                if (double.TryParse(StrValue, out Result))
                    return Result;
                else
                    return null;
            }
        }

        public double? FrameRate {
            get {
                string StrValue = lastMedia.Get(StreamKind.Video, 0, "FrameRate");
                double Result = 0;
                if (double.TryParse(StrValue, out Result))
                    return Result;
                else
                    return null;
            }
        }

        public string AudioFormat {
            get {
                return lastMedia.Get(StreamKind.Audio, 0, "Format");
            }
        }

        public static bool HasDimensions(Media item) {
            return item.MediaTypeId == (int)MediaType.Video || item.MediaTypeId == (int)MediaType.Image;
        }

        public static bool HasMissingInfo(Media item) {
            return (item.FileName != null && 
                File.Exists(Settings.NaturalGroundingFolder + item.FileName) 
                && (item.Length == null || (HasDimensions(item) && item.Height == null)));
        }

        /// <summary>
        /// Releases resources allocated to Windows Media Player COM object.
        /// </summary>
        public void Dispose() {
            // viewer.Source = null;
            //viewer.Dispose();
        }



        private MediaInfo ReadMediaFile(string fileName) {
            //Initilaizing MediaInfo
            MediaInfo MI = new MediaInfo();

            //From: preparing an example file for reading
            FileStream From = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            //From: preparing a memory buffer for reading
            byte[] From_Buffer = new byte[64 * 1024];
            int From_Buffer_Size; //The size of the read file buffer

            //Preparing to fill MediaInfo with a buffer
            MI.Open_Buffer_Init(From.Length, 0);

            //The parsing loop
            do {
                //Reading data somewhere, do what you want for this.
                From_Buffer_Size = From.Read(From_Buffer, 0, 64 * 1024);

                //Sending the buffer to MediaInfo
                System.Runtime.InteropServices.GCHandle GC = System.Runtime.InteropServices.GCHandle.Alloc(From_Buffer, System.Runtime.InteropServices.GCHandleType.Pinned);
                IntPtr From_Buffer_IntPtr = GC.AddrOfPinnedObject();
                Status Result = (Status)MI.Open_Buffer_Continue(From_Buffer_IntPtr, (IntPtr)From_Buffer_Size);
                GC.Free();
                if ((Result & Status.Finalized) == Status.Finalized)
                    break;

                //Testing if MediaInfo request to go elsewhere
                if (MI.Open_Buffer_Continue_GoTo_Get() != -1) {
                    Int64 Position = From.Seek(MI.Open_Buffer_Continue_GoTo_Get(), SeekOrigin.Begin); //Position the file
                    MI.Open_Buffer_Init(From.Length, Position); //Informing MediaInfo we have seek
                }
            }
            while (From_Buffer_Size > 0);

            //Finalizing
            MI.Open_Buffer_Finalize(); //This is the end of the stream, MediaInfo must finnish some work

            return MI;
        }
    }
}

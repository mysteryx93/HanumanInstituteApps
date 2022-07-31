using System.Runtime.InteropServices;
using ManagedBass;
using ManagedBass.Enc;
// ReSharper disable CommentTypo

namespace HanumanInstitute.BassAudio
{
    /// <summary>
    /// BassEnc_Acc is an extension to the BassEnc add-on that allows BASS channels to be Acc encoded, with support for AACENC options.
    /// </summary>
    public static class BassEnc_Acc
    {
        const string DllName = "bassenc_aac";

        [DllImport(DllName)]
        static extern int BASS_Encode_AAC_GetVersion();

        /// <summary>
        /// Gets the Version of BassEnc_Aac that is loaded.
        /// </summary>
        public static Version Version => Extensions.GetVersion(BASS_Encode_AAC_GetVersion());

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_AAC_Start(int handle, string options, EncodeFlags flags, EncodeProcedure procedure, IntPtr user);

        /// <summary>
        /// Start Aac Encoding to <see cref="EncodeProcedure"/>.
        /// </summary>
        /// <param name="handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="options">
        /// Encoder options... null = use defaults.
        /// The following AACENC style options are supported: --object-type --bitrate, --vbr.
        /// Anything else that is included will be ignored.
        /// </param>
        /// <param name="flags">A combination of <see cref="EncodeFlags"/>.</param>
        /// <param name="procedure">Optional callback function to receive the encoded data... null = no callback.</param>
        /// <param name="user">User instance data to pass to the callback function.</param>
        /// <returns>The encoder handle is returned if the encoder is successfully started, else 0 is returned. Use <see cref="Bass.LastError"/> to get the error code</returns>
        /// <remarks>
        /// <see cref="BassEnc.EncodeStart(int,string,EncodeFlags,EncoderProcedure,IntPtr)"/> is used internally to apply the encoder to the source channel, so the remarks in its documentation also apply to this function. 
        /// 
        /// <b>Platform-specific</b>
        /// On Windows and Linux, an SSE supporting CPU is required for sample rates other than 48000/24000/16000/12000/8000 Hz.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="handle"/> is not valid</exception>
        /// <exception cref="Errors.SampleFormat">The channel's sample format is not supported by the encoder.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem! </exception>
        public static int Start(int handle, string options, EncodeFlags flags, EncodeProcedure procedure, IntPtr user)
        {
            return BASS_Encode_AAC_Start(handle, options, flags | EncodeFlags.Unicode, procedure, user);
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_AAC_StartFile(int handle, string options, EncodeFlags flags, string fileName);

        /// <summary>
        /// Start AAC Encoding to File.
        /// </summary>
        /// <param name="handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="options">
        /// Encoder options... null = use defaults.
        /// The following AACENC style options are supported: --object-type --bitrate, --vbr.
        /// Anything else that is included will be ignored.
        /// </param>
        /// <param name="flags">A combination of <see cref="EncodeFlags"/>.</param>
        /// <param name="fileName">Output filename... null = no output file.</param>
        /// <returns>The encoder handle is returned if the encoder is successfully started, else 0 is returned. Use <see cref="Bass.LastError"/> to get the error code</returns>
        /// <remarks>
        /// <see cref="BassEnc.EncodeStart(int,string,EncodeFlags,EncoderProcedure,IntPtr)"/> is used internally to apply the encoder to the source channel, so the remarks in its documentation also apply to this function. 
        /// 
        /// <b>Platform-specific</b>
        /// On Windows and Linux, an SSE supporting CPU is required for sample rates other than 48000/24000/16000/12000/8000 Hz.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="handle"/> is not valid</exception>
        /// <exception cref="Errors.SampleFormat">The channel's sample format is not supported by the encoder.</exception>
        /// <exception cref="Errors.Create">The file could not be created.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem! </exception>
        public static int Start(int handle, string options, EncodeFlags flags, string fileName)
        {
            return BASS_Encode_AAC_StartFile(handle, options, flags | EncodeFlags.Unicode, fileName);
        }
    }
}

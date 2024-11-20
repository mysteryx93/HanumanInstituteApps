# Hanuman Institute Apps

This is a suite of audio/video tools built for my personal needs, and made available for everyone.

They were developed using cutting-edge programming practices, and I wrote a tutorial on [How to Build Modern Cross-Platform Apps with .NET](https://github.com/mysteryx93/Modern.Net-Tutorial). I also had to develop several core components, including [HanumanInstitute.MvvmDialogs](https://github.com/mysteryx93/HanumanInstitute.MvvmDialogs/) and [MediaPlayerUI.NET](https://github.com/mysteryx93/MediaPlayerUI.NET).

Supports Windows, Linux and MacOS. Possibly Android and iOS in the future.

**For MIDI support:** download the sound font of your choice in SF2 format ([a list is available here](https://adieusounds.com/blogs/news/15-free-soundfonts-to-elevate-your-music-production-in-2024)). Place the file in the following location (replacing 'myuser' with your actual user)
- Windows: `C:\Users\myuser\AppData\Roaming\Hanuman Institute\midisounds.sf2`
- Linux: `/home/myuser/.config/Hanuman Institute/midisounds.sf2`

To support the development of these apps, look at our new products: [selling very high-frequency jewelry and herbal extracts](https://alchemistgems.com), shipping internationally. Also of interest, we released a [list of all supplements on the market, measuring their efficiency and comparing them.](https://alchemistgems.com/supplements-efficiency-list)

### 432Hz Player: Plays music in 432Hz

Most music is recorded in 440Hz. Many people claim that 432Hz resonates more with the heart whereas 440Hz resonates more with the brain, and that 432Hz music feels better. Try it yourself. Play a music in 440Hz and in 432Hz, and see which one you prefer. Most people choose the 432Hz version, and it's hard to go back to 440Hz.

This application uses a very high-quality pitch-shifting algorithm with low CPU usage. It also auto-detects the music's exact pitch for precise tuning.

[[More Info]](https://github.com/mysteryx93/HanumanInstituteApps/wiki/432hz-Player) [[Download]](https://sourceforge.net/projects/player432Hz/files/)

### 432Hz Batch Converter: Converts and re-encodes music to 432Hz

This application re-encodes your audio files while shifting the pitch to 432Hz. It uses a very high-quality pitch-shifting algorithm.

It solves 2 problems with one stone:

- 432Hz Player allows playing music on the desktop, and this will allow playing them on other devices
- Certain devices like cars have limited support for file formats, and some music needs to be re-encoded into a standard format to work

[[More Info]](https://github.com/mysteryx93/HanumanInstituteApps/wiki/432hz-Batch-Converter) [[Download]](https://sourceforge.net/projects/converter432hz/files/)

### Powerliminals Player: Plays multiple audios simultaneously at varying speeds

Plays Powerliminal audios, which are nearly-silent audios containing very high-frequency energies to play in the background. You can stack 10 to 30 audios at varying speeds to play in the background day and night, selecting the balance of energies that you need.

I can play 25 audios with ~3% CPU usage.

[[More Info]](https://github.com/mysteryx93/HanumanInstituteApps/wiki/Powerliminals-Player) [[Download]](https://sourceforge.net/projects/powerliminals-player/files/) [[Get the Powerliminals Pack]](https://www.spiritualselftransformation.com/powerliminals-nonrivalry)

### Yang YouTube Downloader: Downloads best-quality audio and video from YouTube

Yet another YouTube downloader? Most downloaders do not give the best quality as they choose the wrong streams and re-encode them.

This downloader allows you to get either the MP4 or VP9 videos, and the AAC or Opus audios, without re-encoding to preserve the best quality. While VP9 is 35% more efficient than MP4 for videos, some videos have 40-60% smaller file sizes in VP9 format! It will automatically select the best-quality video based on file sizes. It can even combine MP4 videos with Opus audios in a MKV file, although not all players will support it. I haven't seen any other downloader that can produce a MKV file with the best video and audio without re-encoding anything.

Yang YouTube Downloader also allows to re-encode audios with all the same features as 432Hz Batch Converter!

[[More Info]](https://github.com/mysteryx93/HanumanInstituteApps/wiki/Yang-YouTube-Downloader) [[Download]](https://sourceforge.net/projects/yangdownloader/files/)

## Legacy Apps

Released from 2015-2017 and not yet rewritten. All apps are part of the same package, only for Windows.

[[Download Legacy Apps]](https://sourceforge.net/projects/naturalgroundingplayer/)

### Natural Grounding Player

[Screenshot](https://raw.githubusercontent.com/mysteryx93/NaturalGroundingPlayer/master/Setup/Screenshots/Screenshot1.png) Provides the ultimate media playback experience by sequencing videos based on their energy readings. It contains the best catalog of videos, the best YouTube downloader and the best media encoder you can find.

The primary goal of this application is to automate Natural Grounding sessions by downloading and playing videos in the right sequence to gradually increase the intensity and bring you into higher states of consciousness. It is a spiritual awakening tool.

[Read more in the Wiki.](https://github.com/mysteryx93/NaturalGroundingPlayer/wiki)

**Note: the video download feature is broken!** The only way to use this app is to bind your database to video files manually.

### Yin Media Encoder

[Screenshot](https://raw.githubusercontent.com/mysteryx93/NaturalGroundingPlayer/master/Setup/Screenshots/Screenshot3.png) The media encoder is one piece of the software that will be very useful for anyone looking to increase the quality of their videos. It allows upscaling and re-encoding videos from SD to HD. It allows to convert your videos to 60fps with just a few clicks. Under the hood, it uses a combination of AviSynth, ffmpeg and x264. With this Media Encoder, anyone can easily re-encode 50 videos in a row. The video will be optimized and you will preserve the original audio.

Here's a sample video <a href="https://www.spiritualselftransformation.com/files/media-encoder-old.mpg">before</a> and <a href="https://www.spiritualselftransformation.com/files/media-encoder-new.mkv">after</a>.

[How to Convert Videos to 60fps](https://github.com/mysteryx93/NaturalGroundingPlayer/wiki/Convert-Videos-to-60fps)

[How to Rip and Optimize VCDs](https://github.com/mysteryx93/NaturalGroundingPlayer/wiki/How-to-Rip-VCDs)

[How to Rip and Optimize DVDs](https://github.com/mysteryx93/NaturalGroundingPlayer/wiki/How-to-Rip-DVDs)

## License

Hanuman Institute Apps is an Open Source project distributed under the <a href="https://github.com/mysteryx93/NaturalGroundingPlayer/blob/master/LICENSE.md">MIT license</a>.

The Natural Grounding Player is built with C#/.NET and Avalonia UI.

## About the author

Brought to you by [Etienne Charland aka Hanuman](https://www.spiritualselftransformation.com/). Made by a Lightworker in his spare time.

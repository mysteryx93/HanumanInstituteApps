# Natural Grounding Player
Provides the ultimate media playback experience by sequencing videos based on their energy readings. It contains the best catalog of videos, the best YouTube downloader and the best media encoder you can find.

The primary goal of this application is to automate Natural Grounding sessions by downloading and playing videos in the right sequence to gradually increase the intensity and bring you into higher states of consciousness. It is a spiritual awakening tool.

To learn more about Natural Grounding, you can read the <a href="https://www.shamanicattraction.com/files/ngguide2011.pdf">2011 Natural Grounding Guide</a> or visit the <a href="http://www.naturalgrounding.net/">Natural Grounding website</a>.

Natural Grounding is a very deep topic. I'm currently writing two books to properly explain what it truly is that will be published on Amazon. [Read more](https://github.com/mysteryx93/NaturalGroundingPlayer/wiki/What-Is-Natural-Grounding)

[Join the Natural Grounding community on Facebook here.](https://www.facebook.com/groups/sxenergytransformationalentertainment/)

## Screenshots
<a href="https://raw.githubusercontent.com/mysteryx93/NaturalGroundingPlayer/master/Setup/Screenshots/Screenshot1.png">Main Window</a>

<a href="https://raw.githubusercontent.com/mysteryx93/NaturalGroundingPlayer/master/Setup/Screenshots/Screenshot2.png">Playlist Editor</a>

<a href="https://raw.githubusercontent.com/mysteryx93/NaturalGroundingPlayer/master/Setup/Screenshots/Screenshot3.png">Media Encoder</a>

## Best Video Catalog

The Natural Grounding Player contains the best database of videos with high energies. 655 videos have energy readings, 375 of which are available on YouTube. Eventually, this will evolve into an online platform where everybody can share and add resources.

## Best Video Downloader

The Natural Grounding Player automatically downloads the videos before playing, or downloads any video on-demand. It performs the downloads in a way that goes beyond any other downloader to get the optimal quality available from YouTube.

YouTube now has two main video formats: WebM (VP9 video + Vorbis audio) and MP4 (H264 video + AAC audio). While WebM supposedly stores twice more data per bit, some of the videos are 40-60% smaller in WebM format than in MP4 format! That's good for live streaming, but the quality looks bad. According to my tests, the WebM format looks better when its file size is no more than 35% smaller than the MP4 format. The Vorbis audio, however, has a considerably higher quality than ACC. Keep in mind that YouTube recently lowered the bitrate of MP4 videos, so if you downloaded videos before that change, you might want to keep those videos.

The upcoming v1.2 will download according to those rules. If WebM is no more than 35% smaller than MP4, download in WebM format. Otherwise, if WebM is available, download H264 video + Vorbis audio into a MKV file (which allows combining any video and audio formats). If WebM is unavailable, download in MP4. There is also a mass-downloader and updater that scans your playlist to detect those that have higher versions available and downloads them all at once.

## Best Media Encoder

The media encoder is one piece of the software that will be very useful for anyone looking to increase the quality of their videos. It allows upscaling and re-encoding videos from SD to HD. It allows to convert your videos to 60fps with just a few clicks. Under the hood, it uses a combination of AviSynth, ffmpeg and x264. With this Media Encoder, anyone can easily re-encode 50 videos in a row. The video will be optimized and you will preserve the original audio.

Here's a sample video <a href="https://www.spiritualselftransformation.com/files/media-encoder-old.mpg">before</a> and <a href="https://www.spiritualselftransformation.com/files/media-encoder-new.mkv">after</a>.

## Best Playback Quality

SVP and madVR allow you to greatly enhance your video playback quality, if your computer can handle it. SVP will run on any good CPU and increase the frame rate to 60fps, while madVR will take the most out of your graphic card to resize videos with better algorithms. These components are very powerful but difficult to configure. The SVP Setup Wizard will make it easy and simple. Furthermore, the Natural Grounding Player will enable and disable these components automatically. If one video doesn't play well with SVP, you can configure the player to disable SVP only for that video.

## Installation

<a href="https://github.com/mysteryx93/NaturalGroundingPlayer/releases">Download the Natural Grounding Player v1.1 here</a>

(Optional) Install <a href="http://svp-team.com/">SVP from here</a> and select to install madVR during setup.

## License

The Natural Grounding Player is an Open Source project distributed under the <a href="https://github.com/mysteryx93/NaturalGroundingPlayer/blob/master/Setup/LICENSE.md">BSD license</a>

## Development

The Natural Grounding Player is built with .NET, C#, WPF and SQLite. If you're interested in contributing to the project, <a href="https://github.com/mysteryx93/NaturalGroundingPlayer/wiki/Roadmap">view the Roadmap page</a> and <a href="https://www.spiritualselftransformation.com/about-us/contact-us">contact me</a> to get you started.

## About the author

<a href="https://www.spiritualselftransformation.com">Etienne Charland helps you get the life that you want</a> to enjoy closer connected relationships, increase your income, and finally feel confident with the direction that you are going with your life. Etienne Charland helps you maximize your full potential on all aspects of your life.

Visit the author's website at <a href="https://www.spiritualselftransformation.com">Spiritual Self Transformation</a>

I do not take donations. If you want to contribute financially, you can purchase any of my products and services. Purchasing any premium resources such as Powerliminal Meditations within the player within the "Buy Resources" tab also help fund the project. The "Buy Resources" tab also allows you to purchase VCDs and DVDs that aren't available on YouTube... IF they are available for purchase!

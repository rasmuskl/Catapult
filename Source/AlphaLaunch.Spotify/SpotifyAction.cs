using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaLaunch.Spotify
{
    internal enum SpotifyAction : long
    {
        PlayPause = 0xe0000,
        Mute = 0x80000,
        VolumeDown = 0x90000,
        VolumeUp = 0xa0000,
        Stop = 0xd0000,
        PreviousTrack = 0xc0000,
        NextTrack = 0xb0000
    }
}
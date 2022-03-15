using Discord.Audio;
using System.Diagnostics;
using System.IO;

namespace Discord_Bot.Modules.ListClasses
{
    public class MusicVariables
    {
        public bool Playing { get; set; }

        public bool JoinedVoice { get; set; }

        public IAudioClient AudioClient { get; set; }

        public Process FFmpeg { get; set; }

        public Stream Output { get; set; }

        public AudioOutStream Discord { get; set; }

        public Stopwatch Stopwatch { get; set; }


        public MusicVariables() { }
    }
}

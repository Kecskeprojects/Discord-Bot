using Discord.Audio;
using System.Diagnostics;
using System.IO;

namespace Discord_Bot.Modules.ListClasses
{
    public class Music_var
    {
        public ulong ServerId { get; set; }
        public bool Playing { get; set; }
        public bool JoinedVoice { get; set; }
        public IAudioClient AudioClient { get; set; }
        public Process FFmpeg { get; set; }
        public Stream Output { get; set; }
        public AudioOutStream Discord { get; set; }
        public Stopwatch Stopwatch { get; set; }

        public Music_var(ulong serverId)
        {
            ServerId = serverId;
            Playing = false;
            JoinedVoice = false;
        }
    }
}

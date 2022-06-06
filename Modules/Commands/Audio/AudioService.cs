using Discord.Audio;
using Discord.Commands;
using Discord_Bot.Modules.ListClasses;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Commands.Audio
{
    public class AudioService
    {
        //After CreateYoutubeStream ran, send the audio over to discord, with the given audio settings
        public static async Task Stream(SocketCommandContext context, IAudioClient client, string url)
        {
            ulong sId = context.Guild.Id;
            try
            {
                Global.servers[sId].AudioVars.FFmpeg = CreateYoutubeStream(url);
                Global.servers[sId].AudioVars.Output = Global.servers[sId].AudioVars.FFmpeg.StandardOutput.BaseStream;

                Global.servers[sId].AudioVars.Stopwatch = Stopwatch.StartNew();

                //Audio streaming
                using (Global.servers[sId].AudioVars.Discord = client.CreatePCMStream(AudioApplication.Mixed, Global.Config.Bitrate, 2000))
                {
                    Console.WriteLine("[" + Global.Current_Time() + "]: Audio stream starting!");
                    Global.Logs.Add(new Log("LOG", "Audio stream starting!"));

                    await Global.servers[sId].AudioVars.Output.CopyToAsync(Global.servers[sId].AudioVars.Discord);
                    await Global.servers[sId].AudioVars.Discord.FlushAsync();
                };

                Global.servers[sId].AudioVars.FFmpeg.WaitForExit();
            }
            //Exception thrown with current version of skipping song
            catch (ObjectDisposedException)
            {
                Console.WriteLine("Exception throw when skipping song!");
                Global.Logs.Add(new Log("LOG", "Exception throw when skipping song!"));
            }
            //Exception thrown when bot abruptly leaves voice channel
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("Exception thrown when audio stream is cancelled!");
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "AudioService.cs Stream", ex.ToString()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "AudioService.cs Stream", ex.ToString()));
            }

            if (!Global.servers[sId].AudioVars.FFmpeg.HasExited)
            {
                Global.servers[sId].AudioVars.FFmpeg.WaitForExit();
            }
            Global.servers[sId].AudioVars.Stopwatch.Stop();

            Console.WriteLine("[" + Global.Current_Time() + "]: Audio stream finished!");
            Global.Logs.Add(new Log("LOG", "Audio stream finished!"));
            return;
        }



        //Use yt-dlp and ffmpeg to stream audio from youtube to discord
        private static Process CreateYoutubeStream(string url)
        {
            ProcessStartInfo ffmpeg = new()
            {
                FileName = "cmd.exe",
                Arguments = $@"/C yt-dlp.exe --no-check-certificate -f bestaudio -o - {url} | ffmpeg.exe -i pipe:0 -f s16le -ar 48000 -ac 2 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Console.WriteLine("Yt-dlp audio stream created for '" + url + "' !");
            Global.Logs.Add(new Log("LOG", "Yt-dlp audio stream created for '" + url + "' !"));
            return Process.Start(ffmpeg);
        }
    }
}

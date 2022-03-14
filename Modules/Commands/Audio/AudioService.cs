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
        static int count = 0;
        public static async Task Stream(SocketCommandContext context, IAudioClient client, string url)
        {
            ulong sId = context.Guild.Id;
            try
            {
                if (!Check_Video(context, url)) return;

                while (true)
                {
                    Global.servers[sId].AudioVars.FFmpeg = CreateYoutubeStream(url);
                    Global.servers[sId].AudioVars.Output = Global.servers[sId].AudioVars.FFmpeg.StandardOutput.BaseStream;

                    Global.servers[sId].AudioVars.Stopwatch = Stopwatch.StartNew();

                    //Audio streaming
                    using (Global.servers[sId].AudioVars.Discord = client.CreatePCMStream(AudioApplication.Mixed, Program.Config.Bitrate, 2000))
                    {
                        Console.WriteLine("[" + Global.Current_Time() + "]: Audio stream starting!");
                        Global.Logs.Add(new Log("LOG", "Audio stream starting!"));

                        try { await Global.servers[sId].AudioVars.Output.CopyToAsync(Global.servers[sId].AudioVars.Discord); }
                        finally { await Global.servers[sId].AudioVars.Discord.FlushAsync(); }
                    };

                    Global.servers[sId].AudioVars.Stopwatch.Stop();

                    //In case youtube-dl comes back with an exit code of 1, try playing the song again, the reason for it returning with 1 is unknown but it does it randomly
                    if (Global.servers[sId].AudioVars.FFmpeg.ExitCode != 0 && count < 3)
                    {
                        Console.WriteLine("Something went wrong with the ffmpeg process! EXIT CODE: " + Global.servers[sId].AudioVars.FFmpeg.ExitCode + " Tries: " + (count + 1));
                        Global.Logs.Add(new Log("WARNING", "Something went wrong with the ffmpeg process! EXIT CODE: " + Global.servers[sId].AudioVars.FFmpeg.ExitCode + " Tries: " + (count + 1)));
                        if (Global.servers[sId].AudioVars.FFmpeg.ExitCode == 1)
                        {
                            Global.servers[sId].AudioVars.FFmpeg.WaitForExit();
                            count++;
                            await Stream(context, client, url);
                        }
                        else break;
                    }
                    else break;
                }
                
                Global.servers[sId].AudioVars.FFmpeg.WaitForExit();
                count = 0;

                Console.WriteLine("[" + Global.Current_Time() + "]: Audio stream finished!");
                Global.Logs.Add(new Log("LOG", "Audio stream finished!"));

                return;
            }
            //Exception thrown with current version of skipping song
            catch(ObjectDisposedException)
            {
                Console.WriteLine("Exception throw when skipping song!");
                Global.Logs.Add(new Log("LOG", "Exception throw when skipping song!"));
                Global.servers[sId].AudioVars.FFmpeg.WaitForExit();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "AudioService.cs Stream", ex.ToString()));
            }

            Console.WriteLine("[" + Global.Current_Time() + "]: Audio stream finished!");
            Global.Logs.Add(new Log("LOG", "Audio stream finished!"));
            Global.servers[sId].AudioVars.Stopwatch.Stop();
            return;
        }



        //Use youtube-dl and ffmpeg to stream audio from youtube to discord
        private static Process CreateYoutubeStream(string url)
        {
            ProcessStartInfo ffmpeg = new()
            {
                FileName = "cmd.exe",
                Arguments = $@"/C youtube-dl.exe --no-check-certificate -f bestaudio -o - {url} | ffmpeg.exe -i pipe:0 -f s16le -ar 48000 -ac 2 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Console.WriteLine("Youtube-dl audio stream created for '" + url + "' !");
            Global.Logs.Add(new Log("LOG", "Youtube-dl audio stream created for '" + url + "' !"));
            return Process.Start(ffmpeg);
        }



        //Check the video before playing it, to root out age and country restricted content
        private static bool Check_Video(SocketCommandContext context, string url)
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = $@"/C youtube-dl.exe --no-check-certificate -F {url}",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };
            process.Start();

            string Error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (Error.Length == 0) return true;
            else
            {
                if (Error.Contains("ERROR: Sign in to confirm your age"))
                {
                    context.Channel.SendMessageAsync("This video is age restricted, song skipped!");
                }
                else if (Error.Contains("ERROR: The uploader has not made this video available in your country."))
                {
                    context.Channel.SendMessageAsync("This video is country-restricted in the bot's current location!");
                }
                else
                {
                    Console.WriteLine(Error);
                    Global.Logs.Add(new Log("ERROR", "AudioService.cs Check_Video", Error));
                }
                return false;
            }
        }
    }
}

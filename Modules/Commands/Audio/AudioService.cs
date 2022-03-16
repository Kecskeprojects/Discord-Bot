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
                //Check video  restriction
                if (!CheckVideo(context, url)) return;

                //Looping in case video could not play correctly the first time
                //In most cases, it only runs once, this is done for edge cases,
                //to a maximum of 3 loops, which seems to be more than enough
                while (true)
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

                    //In case youtube-dl comes back with an exit code of 1,
                    //Do not exit the loop, in any other case, exit the loop,
                    //the reason for it returning with 1 is unknown but it does it seemingly randomly
                    if (Global.servers[sId].AudioVars.FFmpeg.ExitCode != 0 && count < 3)
                    {
                        Console.WriteLine("Something went wrong with the ffmpeg process! EXIT CODE: " + Global.servers[sId].AudioVars.FFmpeg.ExitCode + " Tries: " + (count + 1));
                        Global.Logs.Add(new Log("WARNING", "Something went wrong with the ffmpeg process! EXIT CODE: " + Global.servers[sId].AudioVars.FFmpeg.ExitCode + " Tries: " + (count + 1)));
                        if (Global.servers[sId].AudioVars.FFmpeg.ExitCode == 1)
                        {
                            count++;
                        }
                        else break;
                    }
                    else break;
                }
            }
            //Exception thrown with current version of skipping song
            catch(ObjectDisposedException)
            {
                Console.WriteLine("Exception throw when skipping song!");
                Global.Logs.Add(new Log("LOG", "Exception throw when skipping song!"));
            }
            //Exception thrown when bot abruptly leaves voice channel
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "AudioService.cs Stream", ex.ToString()));
            }

            Global.servers[sId].AudioVars.FFmpeg.WaitForExit();
            Global.servers[sId].AudioVars.Stopwatch.Stop();
            count = 0;

            Console.WriteLine("[" + Global.Current_Time() + "]: Audio stream finished!");
            Global.Logs.Add(new Log("LOG", "Audio stream finished!"));
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
        //Returns true if video is downloadable, false if not
        private static bool CheckVideo(SocketCommandContext context, string url)
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

            //If the error stream is empty, return true, else, send back the issue to the user
            if (Error.Length == 0) return true;
            else
            {
                if (Error.Contains("ERROR: Sign in to confirm your age"))
                {
                    context.Channel.SendMessageAsync("This video is age restricted, song skipped!");
                    Global.Logs.Add(new Log("LOG", "This video is age restricted, song skipped!"));
                }
                else if (Error.Contains("ERROR: The uploader has not made this video available in your country."))
                {
                    context.Channel.SendMessageAsync("This video is country-restricted in the bot's current location!");
                    Global.Logs.Add(new Log("LOG", "This video is country-restricted in the bot's current location!"));
                }
                else
                {
                    Console.WriteLine(Error);
                    Global.Logs.Add(new Log("DEV", "AudioService.cs CheckVideo", Error));
                }
                return false;
            }
        }
    }
}

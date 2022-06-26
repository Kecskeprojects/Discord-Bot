using Discord;
using Discord.WebSocket;
using Discord_Bot.Modules.ListClasses;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.API
{
    public class InstagramAPI
    {
        private static IInstaApi InstaApi;

        //Login or load in for instagram api
        public static async Task<bool> Startup()
        {
            try
            {
                // create user session data and provide login details
                var userSession = new UserSessionData
                {
                    UserName = Global.Config.Instagram_Username,
                    Password = Global.Config.Instagram_Password
                };

                var delay = RequestDelay.FromSeconds(2, 2);
                // create new InstaApi instance using Builder
                InstaApi = InstaApiBuilder.CreateBuilder()
                    .SetUser(userSession)
                    .UseLogger(new DebugLogger(LogLevel.Exceptions)) // use logger for requests and debug messages
                    .SetRequestDelay(delay)
                    .Build();

                //Filepath of user sessions
                const string stateFile = "Assets\\Last_instagram_session.bin";

                try
                {
                    //Check if file exists, if it does, load it
                    if (File.Exists(stateFile))
                    {
                        Console.WriteLine("Loading state from file");
                        Global.Logs.Add(new Log("LOG", "Loading state from file"));
                        using var fs = File.OpenRead(stateFile);
                        InstaApi.LoadStateDataFromStream(fs);
                    }
                }
                catch (Exception e) { Console.WriteLine(e); }

                //If the loaded user is authenticated, we don't have to log in
                if (!InstaApi.IsUserAuthenticated)
                {
                    // login
                    Console.WriteLine($"Logging in as {userSession.UserName}");
                    Global.Logs.Add(new Log("LOG", $"Logging in as {userSession.UserName}"));
                    delay.Disable();
                    var logInResult = await InstaApi.LoginAsync();
                    delay.Enable();
                    if (!logInResult.Succeeded)
                    {
                        Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                        Global.Logs.Add(new Log("ERROR", $"Unable to login: {logInResult.Info.Message}"));
                        return false;
                    }
                }

                //Reading current session
                var state = InstaApi.GetStateDataAsStream();

                //Saving Current session
                using var fileStream = File.Create(stateFile);
                state.Seek(0, SeekOrigin.Begin);
                state.CopyTo(fileStream);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "InstagramAPI.cs Startup", ex.ToString()));
            }

            return false;
        }



        //Handling instagram posts
        public static async Task PostEmbed(ISocketMessageChannel channel, string url)
        {
            try
            {
                int counter = 0;
                while (counter < 3)
                {
                    //Make a url out of message
                    Uri uri = new(url);

                    //Get the id of the media from the given url
                    var first_res = InstaApi.MediaProcessor.GetMediaIdFromUrlAsync(uri).GetAwaiter().GetResult();

                    if (first_res.Succeeded)
                    {
                        //If id could be retrieved, get the media by that id
                        var second_res = InstaApi.MediaProcessor.GetMediaByIdAsync(first_res.Value).GetAwaiter().GetResult();

                        if (second_res.Succeeded)
                        {
                            //If that succeded too, proceed to build the embed
                            var content = second_res.Value;

                            EmbedBuilder builder = new();

                            builder.WithAuthor(content.User.UserName, content.User.ProfilePicture, url);

                            if (content.Caption != null)
                            {
                                builder.WithDescription(content.Caption.Text);
                                builder.WithFooter("instagram  Created:");
                                builder.WithTimestamp(content.Caption.CreatedAtUtc);
                            }
                            else
                            {
                                builder.WithFooter("instagram");
                                builder.WithCurrentTimestamp();
                            }

                            //The RGB(206,0,113) is the color used by discord's default embeds
                            builder.WithColor(206, 0, 113);

                            List<Embed> Embeds = ImageEmbeds(builder, content, url);

                            await channel.SendMessageAsync("", embeds: Embeds.ToArray());

                            break;
                        }
                        else
                        {
                            if (second_res.Info.ResponseType == ResponseType.UnExpectedResponse)
                            {
                                Console.WriteLine("Unexpected Response!");
                                Global.Logs.Add(new Log("ERROR", "Unexpected Response!"));
                                counter++;
                                await Task.Delay(1000);
                            }
                            else
                            {
                                ErrorLogger(first_res.Info);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (first_res.Info.ResponseType == ResponseType.UnExpectedResponse)
                        {
                            Console.WriteLine("Unexpected Response!");
                            Global.Logs.Add(new Log("ERROR", "Unexpected Response!"));
                            counter++;
                            await Task.Delay(1000);
                        }
                        else
                        {
                            ErrorLogger(first_res.Info);
                            break;
                        }
                    }
                }

                if(counter >= 3)
                {
                    await channel.SendMessageAsync("Instagram api temporarily unavailable, try again in a little bit!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "InstagramAPI.cs PostEmbed", ex.ToString()));
            }
        }



        //Handling instagram stories
        public static async Task StoryEmbed(ISocketMessageChannel channel, string url)
        {
            try
            {
                int counter = 0;
                while (counter < 3)
                { //Get the username and the story's id out of the link
                    string username = url.Split("stories/", StringSplitOptions.RemoveEmptyEntries)[1].Split('/')[0];
                    string storyId = url.Split("stories/", StringSplitOptions.RemoveEmptyEntries)[1].Split('/')[1];

                    //Get user info
                    var first_res = InstaApi.UserProcessor.GetUserInfoByUsernameAsync(username).GetAwaiter().GetResult();

                    if (first_res.Succeeded)
                    {
                        //If it succeeded, try getting the user's stories using the user's id
                        var second_res = InstaApi.StoryProcessor.GetUserStoryAsync(first_res.Value.Pk).GetAwaiter().GetResult();

                        if (second_res.Succeeded)
                        {
                            //If that also succeeds, get the value out of it
                            var content = second_res.Value;

                            EmbedBuilder builder = new();

                            builder.WithAuthor(content.User.UserName, content.User.ProfilePicture, url);

                            //We go through each story
                            foreach (var story in content.Items)
                            {
                                //The story id's are separated into two parts like so: storyId_userId
                                //We don't need the separating character or the user's id
                                string currStoryId = story.Id.Split('_')[0];

                                //If we found the story in the link, build the embed, depending on it having captions or not, results will differ
                                if (currStoryId == storyId)
                                {
                                    if (story.Caption != null)
                                    {
                                        builder.WithDescription(story.Caption.Text);
                                        builder.WithTimestamp(story.Caption.CreatedAtUtc);
                                        builder.WithFooter("instagram  Created:");
                                    }
                                    else
                                    {
                                        builder.WithFooter("instagram");
                                        builder.WithCurrentTimestamp();
                                    }

                                    //Add thumbnail of story
                                    builder.WithImageUrl(story.ImageList[0].Uri);

                                    break;
                                }
                            }

                            //The RGB(206,0,113) is the color used by discord's default embeds
                            builder.WithColor(206, 0, 113);

                            await channel.SendMessageAsync("", false, builder.Build());

                            break;
                        }
                        else
                        {
                            if (second_res.Info.ResponseType == ResponseType.UnExpectedResponse)
                            {
                                Console.WriteLine("Unexpected Response!");
                                Global.Logs.Add(new Log("ERROR", "Unexpected Response!"));
                                counter++;
                                await Task.Delay(1000);
                            }
                            else
                            {
                                ErrorLogger(first_res.Info);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (first_res.Info.ResponseType == ResponseType.UnExpectedResponse)
                        {
                            Console.WriteLine("Unexpected Response!");
                            Global.Logs.Add(new Log("ERROR", "Unexpected Response!"));
                            counter++;
                            await Task.Delay(1000);
                        }
                        else
                        {
                            ErrorLogger(first_res.Info);
                            break;
                        }
                    }
                }

                if (counter >= 3)
                {
                    await channel.SendMessageAsync("Instagram api temporarily unavailable, try again in a little bit!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "InstagramAPI.cs StoryEmbed", ex.ToString()));
            }
        }



        //Handling instagram profiles
        public static async Task ProfileEmbed(ISocketMessageChannel channel, string url)
        {
            try
            {
                int counter = 0;
                while(counter < 3)
                {//Get the username out of the link
                    string username = url.Split("/", StringSplitOptions.RemoveEmptyEntries)[2];

                    //Search for user info using the username
                    var first_res = InstaApi.UserProcessor.GetUserInfoByUsernameAsync(username).GetAwaiter().GetResult();

                    if (first_res.Succeeded)
                    {
                        //Also get the most recent post
                        var second_res = InstaApi.UserProcessor.GetUserMediaByIdAsync(first_res.Value.Pk, PaginationParameters.MaxPagesToLoad(1)).GetAwaiter().GetResult();

                        if (second_res.Succeeded)
                        {
                            //If both succeded, get the user info into separate variable
                            var userInfo = first_res.Value;

                            EmbedBuilder builder = new();

                            builder.WithTitle(userInfo.Username);
                            builder.WithThumbnailUrl(userInfo.HdProfilePicUrlInfo.Uri);

                            builder.WithDescription($"{userInfo.FullName}\n{userInfo.Biography}");

                            builder.AddField("Posts:", userInfo.MediaCount, true);
                            builder.AddField("Followers:", userInfo.FollowerCount, true);
                            builder.AddField("Following:", userInfo.FollowingCount, true);

                            //If the user doesn't have any posts, the embed will not have an images, and the timestamp will be the current time
                            if (second_res.Value.Count > 0)
                            {
                                var posts = second_res.Value;

                                if (posts[0].Caption != null)
                                {
                                    builder.WithFooter("instagram Created:");
                                    builder.WithTimestamp(posts[0].Caption.CreatedAtUtc);
                                }
                                else
                                {
                                    builder.WithFooter("instagram");
                                    builder.WithCurrentTimestamp();
                                }

                                //The RGB(206,0,113) is the color used by discord's default embeds
                                builder.WithColor(206, 0, 113);

                                List<Embed> Embeds = ImageEmbeds(builder, posts[0], url);

                                await channel.SendMessageAsync("", false, embeds: Embeds.ToArray());
                            }
                            else
                            {
                                builder.WithFooter("instagram");
                                builder.WithCurrentTimestamp();
                                
                                //The RGB(206,0,113) is the color used by discord's default embeds
                                builder.WithColor(206, 0, 113);
                                
                                await channel.SendMessageAsync("", false, builder.Build());
                            }

                            break;
                        }
                        else
                        {
                            if (second_res.Info.ResponseType == ResponseType.UnExpectedResponse)
                            {
                                Console.WriteLine("Unexpected Response!");
                                Global.Logs.Add(new Log("ERROR", "Unexpected Response!"));
                                counter++;
                                await Task.Delay(1000);
                            }
                            else
                            {
                                ErrorLogger(first_res.Info);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (first_res.Info.ResponseType == ResponseType.UnExpectedResponse)
                        {
                            Console.WriteLine("Unexpected Response!");
                            Global.Logs.Add(new Log("ERROR", "Unexpected Response!"));
                            counter++;
                            await Task.Delay(1000);
                        }
                        else
                        {
                            ErrorLogger(first_res.Info);
                            break;
                        }
                    }
                }

                if (counter >= 3)
                {
                    await channel.SendMessageAsync("Instagram api temporarily unavailable, try again in a little bit!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "InstagramAPI.cs ProfileEmbed", ex.ToString()));
            }
        }

        private static void ErrorLogger(ResultInfo Info)
        {
            try
            {
                string message = "Instagram API error:\n" +
                    $"Was Timeout: {Info.Timeout}\n" +
                    $"Info Message: {Info.Message ?? ""}\n" +
                    $"Response Type: {Info.ResponseType}\n" +
                    $"Exception: {Info.Exception ?? new Exception("Unspecified Error")}";

                Console.WriteLine(
                    $"===========================================================\n" +
                    message +
                    $"\n===========================================================");

                Global.Logs.Add(new Log("ERROR", message));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "InstagramAPI.cs ImageEmbeds", ex.ToString()));
            }
        }

        private static List<Embed> ImageEmbeds(EmbedBuilder main, InstaMedia content, string url)
        {
            List<Embed> Embeds = new();

            try
            {
                //Carousels and image and video contents are made differently
                //So we get the image url according to it's type
                switch (content.MediaType)
                {
                    case InstaMediaType.Image:
                    case InstaMediaType.Video:
                        {
                            int length = 4 > content.Images.Count ? content.Images.Count : 4;
                            for (int i = 0; i < length; i++)
                            {
                                if(i == 0)
                                {
                                    Embeds.Add(main.WithUrl(url).WithImageUrl(content.Images[i].Uri).Build());
                                }

                                Embeds.Add(new EmbedBuilder().WithUrl(url).WithImageUrl(content.Images[i].Uri).Build());
                            }
                            break;
                        }
                    case InstaMediaType.Carousel:
                        {
                            int length = 4 > content.Carousel.Count ? content.Carousel.Count : 4;
                            for (int i = 0; i < length; i++)
                            {
                                if (i == 0)
                                {
                                    Embeds.Add(main.WithUrl(url).WithImageUrl(content.Carousel[i].Images[0].Uri).Build());
                                }

                                Embeds.Add(new EmbedBuilder().WithUrl(url).WithImageUrl(content.Carousel[i].Images[0].Uri).Build());
                            }
                            break;
                        }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "InstagramAPI.cs ImageEmbeds", ex.ToString()));
            }

            return Embeds;
        }
    }
}

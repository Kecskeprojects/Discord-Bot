using System.Threading.Tasks;
using Discord_Bot.Modules.API.Lastfm.LastfmClasses;
using Newtonsoft.Json;
using System;
using Discord.Commands;
using Discord;

namespace Discord_Bot.Modules.API.Lastfm
{
    public class LastfmFunctions:LastfmAPI
    {
        //Checking the total plays of a user using the topartist part of the api, as it has the least amount of entries in any case
        public static async Task<int> TotalPlays(string name, string period)
        {
            int page = 1, totalpage, totalplays = 0;
            do
            {
                var temp2 = await StandardRequestHandler("user.gettopartists", name, period: period, page:page);
                var track_res = JsonConvert.DeserializeObject<TopArtistClass.TopArtist>(temp2.Content);

                foreach (var artist in track_res.TopArtists.Artist)
                {
                    totalplays += int.Parse(artist.PlayCount);
                }

                totalpage = int.Parse(track_res.TopArtists.Attr.TotalPages);
                page++;
            } while (page <= totalpage);

            return totalplays;
        }




        //Checks the inputs for certain lastfm commands, so that they are ordered properly
        public static string[] LastfmParameterCheck(string[] parameters)
        {
            string[] outarray = new string[] { "", "" }; ;

            switch (parameters.Length)
            {
                //If both parameters are given, we check if they are in the right order
                case 2:
                    {
                        //If first is number and second is part of array, input was in correct order
                        if (int.TryParse(parameters[0], out _) && LastfmTimePeriod(parameters[1], out parameters[1]))
                        {
                            outarray = new string[] { parameters[0], parameters[1] };
                        }
                        //If first is part of array and second is number, input is switched
                        else if (int.TryParse(parameters[1], out _) && LastfmTimePeriod(parameters[0], out parameters[0]))
                        {
                            outarray = new string[] { parameters[1], parameters[0] };
                        }
                        //Else the input was wrong
                        else throw new Exception("Wrong input format!");
                        break;
                    }
                //If only one of them is given, we figure out the other and give default value to the other
                case 1:
                    {
                        //If parameter is number, we give the other default value
                        if (int.TryParse(parameters[0], out _))
                        {
                            outarray[0] = parameters[0];
                            outarray[1] = "overall";
                        }
                        //If parameter is part of array, we give the other default value
                        else if (LastfmTimePeriod(parameters[0], out parameters[0]))
                        {
                            outarray[0] = "10";
                            outarray[1] = parameters[0];
                        }
                        //Else the input was wrong
                        else throw new Exception("Wrong input format!");
                        break;
                    }
                //If no parameters are given, we give default values
                case 0:
                    {
                        outarray[0] = "10";
                        outarray[1] = "overall";
                        break;
                    }
                //In every other case, we throw an error
                default:
                    throw new Exception("Too many or too few parameters!");
            }

            return outarray;
        }



        //Simple converter to lastfm API accepted formats
        public static bool LastfmTimePeriod(string input, out string period)
        {
            switch (input.ToLower())
            {
                case "overall":
                case "alltime":
                case "all":
                    {
                        period = "overall";
                        return true;
                    }
                case "week":
                case "1week":
                case "7day":
                case "7days":
                    {
                        period = "7day";
                        return true;
                    }
                case "30day":
                case "30days":
                case "month":
                case "1month":
                case "1months":
                    {
                        period = "1month";
                        return true;
                    }
                case "quarter":
                case "3month":
                case "3months":
                    {
                        period = "3month";
                        return true;
                    }
                case "half":
                case "6month":
                case "6months":
                    {
                        period = "6month";
                        return true;
                    }
                case "year":
                case "1year":
                case "12month":
                case "12months":
                    {
                        period = "12month";
                        return true;
                    }
                default:
                    {
                        period = null;
                        return false;
                    }
            }
        }



        public static string GetNickName(SocketCommandContext context)
        {
            //Only check for nickname if user is not using DMs
            if (context.Channel.GetChannelType() != ChannelType.DM)
            {
                //If user has a nickname, use that in the embed
                return (context.User as Discord.WebSocket.SocketGuildUser).Nickname ?? context.User.Username;
            }
            else return context.User.Username;
        }



        public static EmbedBuilder BaseEmbed(string HeadText, string image_url = "")
        {
            //Building embed
            EmbedBuilder builder = new();

            builder.WithAuthor(HeadText, iconUrl: "https://cdn.discordapp.com/attachments/891418209843044354/923401581704118314/last_fm.png");

            if(image_url != "")
            {
                builder.WithThumbnailUrl(image_url);
            }

            builder.WithCurrentTimestamp();
            builder.WithColor(Color.Red);

            return builder;
        }
    }
}

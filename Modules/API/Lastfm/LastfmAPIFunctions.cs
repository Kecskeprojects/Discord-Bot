using System.Threading.Tasks;
using Discord_Bot.Modules.API.Lastfm.LastfmClasses;
using RestSharp;
using Newtonsoft.Json;
using System.Linq;
using System;

namespace Discord_Bot.Modules.API.Lastfm
{
    public class LastfmFunctions
    {
        //Checking the total plays of a user using the topartist part of the api, as it has the least amount of entries in any case
        public static async Task<int> TotalPlays(RestClient _client, string name, string period)
        {
            int page = 1, totalpage, totalplays = 0;
            do
            {
                var track_req = new RestRequest($"?method=user.gettopartists&user={name}&api_key={Program.Config.Lastfm_API_Key}&limit=1000&period={period}&page={page}&format=json");
                var temp2 = await _client.GetAsync(track_req);
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
        public static string[] TopLastfmCheck(string[] parameters)
        {
            string[] periods = new string[] { "overall", "7day", "1month", "3month", "6month", "12month" };

            string[] outarray;
            if (parameters.Length == 2)
            {
                //If both parameters are given, we check if they are in the right order
                if (int.TryParse(parameters[0], out _) && periods.Contains(parameters[1]))
                {
                    outarray = new string[] { parameters[0], parameters[1] };
                }
                else if (int.TryParse(parameters[1], out _) && periods.Contains(parameters[0]))
                {
                    outarray = new string[] { parameters[1], parameters[0] };
                }
                else throw new Exception("Wrong input format!");
            }
            else if (parameters.Length == 1)
            {
                //If only one of them is given, we figure out the other and give default value to the other
                outarray = new string[] { "", "" };
                if (int.TryParse(parameters[0], out _))
                {
                    outarray[0] = parameters[0];
                    outarray[1] = "overall";
                }
                else if (periods.Contains(parameters[0]))
                {
                    outarray[0] = "10";
                    outarray[1] = parameters[0];
                }
                else throw new Exception("Wrong input format!");
            }
            //In every other case, we throw an error
            else throw new Exception("Too many or too few parameters!");

            return outarray;
        }
    }
}

using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Linq;
using Discord_Bot.Modules.ListClasses;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.API.Lastfm
{

    public class PictureHandler
    {
        //Our image size will be 800*500 in the end
        public static string EditPicture(string image_url, Dictionary<string, int> plays, string HeadText)
        {
            try
            {//Get two instances of the same picture
                using Image mainImage = Image.Load(image_url);
                using Image AlbumImage = Image.Load(image_url);


                //Get the dominant and contrast colors for the album image
                Tuple<Color, Color> Colors = GetContrastAndDominantColors(Image.Load<Rgba32>(image_url));
                Color DominantColor = Colors.Item1;
                Color ContrastColor = Colors.Item2;
                

                //Resize the album picture that is not blurred, 1 pixel less on each side so it can have a proper border
                AlbumImage.Mutate(x => x.Resize(348, 348));

                //Resize the blurred image, cut it to size, cutting off 150pixels from the top and the bottom
                //Then lastly do a heavy blur on it
                mainImage.Mutate(x =>
                    x.Resize(800, 800)
                    .Crop(new Rectangle(0, 150, 800, 500))
                    .GaussianBlur(15)
                );

                //Set oppacity of drawn single color rectangles
                DrawingOptions options = new();
                options.GraphicsOptions.BlendPercentage = 0.5F;

                //Rectangle behind album picture
                mainImage.Mutate(x =>
                    x.Fill(options, ContrastColor, new Rectangle(75, 125, 350, 350))
                );

                //Rectangle behind users, starting slightly lower and a little shorter+
                mainImage.Mutate(x =>
                    x.Fill(options, ContrastColor, new Rectangle(425, 132, 300, 333))
                );

                //Place Album image, 1 pixel lower and to the right
                mainImage.Mutate(x => x.DrawImage(AlbumImage, location: new Point(76, 126), 1));

                //Base font for top text
                Font font = SystemFonts.CreateFont("Bot Font", 40, FontStyle.Regular);

                //Get what color should be used for head text
                Color TextColor = BlackOrWhite(DominantColor);

                //We write the artist and the song in separate lines in case it turns out too long
                string[] HeadTextparts = HeadText.Replace(" by ", "\nby ").Split("\n");
                for(int i = 0; i < HeadTextparts.Length; i++)
                {
                    //Measure the length of the text so we can put it in the middle
                    var textsize = TextMeasurer.MeasureBounds(HeadTextparts[i], new TextOptions(font));

                    int X = (mainImage.Width - (int)textsize.Width) / 2;
                    int Y = (125 - (int)textsize.Height * HeadTextparts.Length) / 2 + (int)textsize.Height * i;

                    //Put Top text on image
                    mainImage.Mutate(x =>
                        x.DrawText(HeadTextparts[i], font, TextColor, new Point(X, Y))
                    );
                }

                //Modify graphics option to have a higher oppacity
                options.GraphicsOptions.BlendPercentage = 0.8F;

                //Modify font to be smaller and not bold
                font = SystemFonts.CreateFont("Bot Font", 14, FontStyle.Regular);

                //Get what text color should be used for the leaderboard
                TextColor = BlackOrWhite(ContrastColor);

                int length = plays.Count > 12 ? 12 : plays.Count;

                for (int i = 0; i < length; i++)
                {
                    //Put dictionary values into temporary variables
                    string user = $"{i + 1}# {plays.Keys.ToArray()[i]}";
                    int playCount = plays[plays.Keys.ToArray()[i]];

                    //Place slightly less see through rectangle behind text
                    mainImage.Mutate(x =>
                        x.Fill(options, ContrastColor, new Rectangle(425, 132 + i * 28, 300, 25))
                    );

                    //Check the length of the user string
                    var textsize = TextMeasurer.MeasureBounds(string.Format("{0, 12}", user), new TextOptions(font));

                    //If it is longer than 210 it will collide with the plays
                    if (textsize.Width > 210)
                    {
                        //We check character by character when it is shorter than that limit
                        for (int ch = user.Length; ch > 0; ch--)
                        {
                            //We check the currently shortened string's length
                            float tempwidth = TextMeasurer.MeasureBounds(string.Format("{0, 12}", user[..ch]), new TextOptions(font)).Width;

                            //When it is short enough, we shorten the original text to this version and move on
                            if (tempwidth < 210)
                            {
                                user = user[..ch];
                                break;
                            }
                        }
                    }

                    //Place tranking and name of user
                    mainImage.Mutate(x => x.DrawText(user, font, TextColor, new Point(427, 137 + i * 28)));

                    //Placeholder text, formatting is permanent though
                    string points = string.Format("{0,12}", $"{playCount} plays");
                    textsize = TextMeasurer.MeasureBounds(string.Format("{0, 12}", points), new TextOptions(font));
                    
                    //Amount of plays the user has
                    mainImage.Mutate(x =>
                        x.DrawText(points, font, TextColor, new Point(722 - (int)textsize.Width, 137 + i * 28))
                    );
                }

                string fileName = $"{new Random().Next(0, int.MaxValue)}.png";

                mainImage.Save(fileName); // Automatic encoder selected based on extension.

                return fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmPictureDrawer.cs EditPicture", ex.ToString()));
            }

            return "";
        }


        public static Tuple<Color, Color> GetContrastAndDominantColors(Image<Rgba32> image)
        {
            //Use nearest neighbor sampling to simplify image and resize it to a 100 pixel wide image with near equal aspect ratio
            image.Mutate(x => x
                .Resize(new ResizeOptions
                {
                    Sampler = KnownResamplers.NearestNeighbor,
                    Size = new Size(100, 0)
                }
                )
            );

            //Declare variables
            int r = 0, g = 0, b = 0;
            int totalPixels = 0;

            //Go through the image pixel line by pixel line
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var pixel = image[x, y];

                    r += Convert.ToInt32(pixel.R);
                    g += Convert.ToInt32(pixel.G);
                    b += Convert.ToInt32(pixel.B);

                    totalPixels++;
                }
            }

            //Putpixel numbers into the 255 range by dividing it with the total pixels checked
            r /= totalPixels;
            g /= totalPixels;
            b /= totalPixels;

            //We get the dominant color
            Rgba32 DominantColor = new((byte)r, (byte)g, (byte)b, 255);

            //By subtracting the values from 255 we get the exact opposite of the color, making it the most visible color
            r = 255 - r; g = 255 - g; b = 255 - b;
            Rgba32 ContrastColor = new((byte)r, (byte)g, (byte)b, 255);

            //Return the two colors in a tuple
            return new Tuple<Color, Color>(DominantColor, ContrastColor);
        }


        public static Rgba32 BlackOrWhite(Rgba32 color)
        {
            //Formula supposedly deciding whether black or white would look more visible
            var l = 0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B;

            return l < 127.5 ? Color.White : Color.Black;
        }


        public static async Task<string> DownloadImageAsync(string directoryPath, string fileName, Uri uri)
        {
            try
            {
                using var httpClient = new HttpClient();

                // Get the file extension
                var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                var fileExtension = Path.GetExtension(uriWithoutQuery);

                // Create file path and ensure directory exists
                var path = Path.Combine(directoryPath, $"{fileName}{fileExtension}");

                // Download the image and write to the file
                var imageBytes = await httpClient.GetByteArrayAsync(uri);
                await File.WriteAllBytesAsync(path, imageBytes);

                return path;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmPictureDrawer.cs DownloadImageAsync", ex.ToString()));
            }
            return "";
        }
    }
}

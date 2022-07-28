using AutoIt;
using ImageFinderNS;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace downbad
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Mat filter(Mat img)
            {
                return img.InRange(new Scalar(250, 250, 250), new Scalar(256, 256, 256));
            }

            Mat filterCheckmark(Mat img)
            {
                return img.InRange(new Scalar(145, 219, 36), new Scalar(149,223,40));
            }

            var speechbubble=filter(Cv2.ImRead("speechbubble.png", ImreadModes.Color));//.ConvertTo(speechbubble, MatType.CV_64FC1,1/255.0f);
            var chatplaying = filter(Cv2.ImRead("chatplaying.png", ImreadModes.Color));//.ConvertTo(speechbubble, MatType.CV_64FC1,1/255.0f);
            var checkmark=filterCheckmark(Cv2.ImRead("checkmark.png", ImreadModes.Color));//.ConvertTo(speechbubble, MatType.CV_64FC1,1/255.0f);
            var result = new Mat();
            var dpiScale = 1.25;
            while (true)
            {
                while (AutoItX.WinActive("Genshin Impact") == 0)
                    Cv2.WaitKey(100);

                var screenshot = ImageFinder.MakeScreenshot();
                var screen = (screenshot.ToMat().CvtColor(ColorConversionCodes.BGRA2BGR));//.ConvertTo(screen, MatType.CV_64FC1,1/255.0f);
                Mat filteredScreen = filter(screen);

                Console.WriteLine("Looking for speech bubble");
                Cv2.MatchTemplate(filteredScreen, speechbubble, result, TemplateMatchModes.SqDiffNormed);
                result.MinMaxLoc(out var score, out var _, out var offset, out var _);
                Console.WriteLine($" Found pos = {offset} score = {score}");

                if (score < 0.1)
                {
                    Console.WriteLine("  Clicking");
                    AutoItX.MouseClick(x: (int)((offset.X + speechbubble.Width / 2) / dpiScale), y: (int)((offset.Y + speechbubble.Height / 2) / dpiScale));
                }
                else
                {
                    Console.WriteLine("Looking for checkmark");
                    Cv2.MatchTemplate(filterCheckmark( screen), checkmark, result, TemplateMatchModes.SqDiffNormed);
                    result.MinMaxLoc(out score, out var _, out offset, out var _);
                    Console.WriteLine($" Found pos = {offset} score = {score}");

                    if (score < 0.2)
                    {
                        Console.WriteLine("  Clicking");
                        AutoItX.MouseClick(x: (int)((offset.X + speechbubble.Width / 2) / dpiScale), y: (int)((offset.Y + speechbubble.Height / 2) / dpiScale));
                    }
                    else
                    {

                        Console.WriteLine("Checking if in chat mode");
                        Mat corner = filter(screen.SubMat(0, chatplaying.Height, 0, chatplaying.Width));
                        Cv2.MatchTemplate(corner, chatplaying, result, TemplateMatchModes.SqDiffNormed);
                        result.MinMaxLoc(out  score, out var _, out var _, out var _);
                        Console.WriteLine($" Score = {score}");
                        if (score < 0.5)
                        {
                            Console.WriteLine("Nothing found, press space");
                            AutoItX.Send(" ");
                        }
                    }
                }
                Cv2.WaitKey(500);

                screen.Dispose();
                screenshot.Dispose();
            }


        }
    }
}

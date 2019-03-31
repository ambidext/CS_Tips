using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCVSharp_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            VideoCapture capture = new VideoCapture(@"e:\Temp\test.mp4");

            int sleepTime = (int)Math.Round(1000 / capture.Fps);

            using (Window window = new Window("capture"))
            using (Mat image = new Mat()) // Frame image buffer
            {
                // When the movie playback reaches end, Mat.data becomes NULL.
                while (true)
                {
                    capture.Read(image); // same as cvQueryFrame
                    if (image.Empty())
                        break;

                    window.ShowImage(image);
                    Cv2.WaitKey(sleepTime);
                }
            }
        }
    }
}

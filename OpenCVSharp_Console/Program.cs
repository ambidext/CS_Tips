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
            VideoCapture capture = new VideoCapture(@"e:\Temp\test.avi");

            int sleepTime = (int)Math.Round(1000 / capture.Fps);

            String filenameFaceCascade = "../../data/haarcascades/haarcascade_frontalface_alt.xml";
            String filenameBodyCascade = "../../data/haarcascades/haarcascade_fullbody.xml";
            String filenameUpperBodyCascade = "../../data/haarcascades/haarcascade_upperbody.xml";
            CascadeClassifier faceCascade = new CascadeClassifier();
            CascadeClassifier bodyCascade = new CascadeClassifier();
            CascadeClassifier upperBodyCascade = new CascadeClassifier();
            if (!faceCascade.Load(filenameFaceCascade))
            {
                Console.WriteLine("error");
                return;
            }
            if (!bodyCascade.Load(filenameBodyCascade))
            {
                Console.WriteLine("error");
                return;
            }
            if (!upperBodyCascade.Load(filenameUpperBodyCascade))
            {
                Console.WriteLine("error");
                return;
            }

            int frame_index = 0;
            using (Window window = new Window("capture"))
            using (Mat image = new Mat()) // Frame image buffer
            {
                while (true)
                {
                    capture.Set(CaptureProperty.PosFrames, frame_index);
                    frame_index += 33;
                    capture.Read(image); 
                    
                    if (image.Empty())
                        break;

                    // detect 
                    Rect[] faces = faceCascade.DetectMultiScale(image);
                    Rect[] bodies = bodyCascade.DetectMultiScale(image);
                    Rect[] upperBodies = upperBodyCascade.DetectMultiScale(image);

                    foreach (var item in faces)
                    {
                        Cv2.Rectangle(image, item, Scalar.Red); // add rectangle to the image
                        Console.WriteLine("faces : " + item);
                    }
                    foreach (var item in bodies)
                    {
                        Cv2.Rectangle(image, item, Scalar.Blue); // add rectangle to the image
                        Console.WriteLine("bodies : " + item);
                    }
                    foreach (var item in upperBodies)
                    {
                        Cv2.Rectangle(image, item, Scalar.Yellow); // add rectangle to the image
                        Console.WriteLine("upperBodies : " + item);
                    }

                    // display
                    window.ShowImage(image);

                    Cv2.WaitKey(sleepTime);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using OpenCvSharp.Extensions;

namespace OpenCVSharp_Winform
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //  base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            //var g = e.Graphics;
            //Bitmap item;
            //while (true)
            //{
            //    if (_q.TryTake(out item) == true)
            //    {
            //        g.DrawImage(item, new PointF(0, 0));
            //        item.Dispose();
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}
        }

        BlockingCollection<Bitmap> _q = new BlockingCollection<Bitmap>();

        private Thread camera;

        private void CaptureCameraCallback()
        {
            VideoCapture capture = new VideoCapture(@"e:\Temp\test.mp4");

            if (capture.IsOpened() == false)
            {
                return;
            }

            int fps = (int)capture.Fps;

            int expectedProcessTimePerFrame = 1000 / fps;
            Stopwatch st = new Stopwatch();
            st.Start();
            int cnt = 0;
            using (Mat image = new Mat())
            {
                while (true)
                {
                    long started = st.ElapsedMilliseconds;
                    capture.Read(image);

                    if (image.Empty() == true)
                    {
                        break;
                    }
                    string fname = string.Format("test{0:D3}.jpg", cnt++);
                    image.ImWrite(fname);

                    _q.Add(BitmapConverter.ToBitmap(image));
                    //this.Invoke((Action)(() => this.Invalidate()));
                    pictureBox1.Invoke((Action)(() => pictureBox1.Invalidate()));
                    int elapsed = (int)(st.ElapsedMilliseconds - started);
                    int delay = expectedProcessTimePerFrame - elapsed;

                    if (delay > 0)
                    {
                        Thread.Sleep(delay);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            camera = new Thread(CaptureCameraCallback);
            camera.IsBackground = true;
            camera.Start();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            Bitmap item;
            while (true)
            {
                if (_q.TryTake(out item) == true)
                {
                    g.DrawImage(item, new PointF(0, 0));
                    item.Dispose();
                }
                else
                {
                    break;
                }
            }
        }
    }
}

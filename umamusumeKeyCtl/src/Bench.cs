using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Documents;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using umamusumeKeyCtl.Util;
using Size = OpenCvSharp.Size;

namespace umamusumeKeyCtl
{
    public class Bench : IDisposable
    {
        private Bitmap source;

        public Bench(Bitmap source)
        {
            this.source = source;
        }

        public void DoBenchmark(int maxLoop, int sampleCount)
        {
            var stopwatches1 = new List<Stopwatch>();
            var stopwatches2 = new List<Stopwatch>();
            
            for (int i = 0; i < sampleCount; i++)
            {
                Debug.Print("Task #1 start.");

                var doPerformScalingResult = MaskByGraphics(maxLoop);
                stopwatches1.Add(doPerformScalingResult);

                Debug.Print($"Task #1 end. Elapsed time: {doPerformScalingResult.ElapsedMilliseconds}");
            
                Debug.Print("Task #2 start.");

                var doPerformAffinResult = DoByCv2(maxLoop);
                stopwatches2.Add(doPerformAffinResult);

                Debug.Print($"Task #2 end. Elapsed time: {doPerformAffinResult.ElapsedMilliseconds}");
            }

            List<(string, long, long)> results = new();
            
            Debug.Print("Result:");
            Debug.Print("No. | PerformScaling | PerformAffin");
            
            for (int i = 0; i < sampleCount; i++)
            {
                Debug.Print($"#{i} | {stopwatches1[i].ElapsedMilliseconds} | {stopwatches2[i].ElapsedMilliseconds}");
            }

            Debug.Print("---------------------------------------------------------");
            
            Debug.Print($"Average:");
            
            Debug.Print("PerformScaling | PerformAffin");
            
            long average1 = 0;

            foreach (var stopwatch in stopwatches1)
            {
                average1 += stopwatch.ElapsedMilliseconds;
            }

            average1 /= stopwatches1.Count;
            
            long average2 = 0;

            foreach (var stopwatch in stopwatches2)
            {
                average2 += stopwatch.ElapsedMilliseconds;
            }

            average2 /= stopwatches1.Count;
            
            Debug.Print($"{average1} | {average2}");
        }

        private Stopwatch MaskByGraphics(int maxLoop)
        {
            var stopWatch = new Stopwatch();

            var clone = (Bitmap) source.Clone();
            var white = new Bitmap(clone.Width, clone.Height);
            using (Graphics g = Graphics.FromImage(white))
            {
                g.Clear(Color.White);
            }

            stopWatch.Start();
            
            for (int i = 0; i < maxLoop; i++)
            {
                var random = new Random();
                var maskList = new List<Rectangle>();
                for (int j = 0; j < maxLoop/10; j++)
                {
                    var mask = new Rectangle(random.Next(0, clone.Width - 1), random.Next(0, clone.Height - 1), random.Next(1, clone.Width), random.Next(1, clone.Height));
                    maskList.Add(mask);
                }

                var img = new Bitmap(clone.Width, clone.Height);
                using (Graphics graphics = Graphics.FromImage(img))
                {
                    foreach (var rect in maskList)
                    {
                        graphics.DrawImageUnscaledAndClipped(white, rect);
                    }
                }
                
                img.Dispose();
            }

            stopWatch.Stop();
            clone.Dispose();

            return stopWatch;
        }
        
        private Stopwatch DoByCv2(int maxLoop)
        {
            var stopWatch = new Stopwatch();


            var clone = (Bitmap) source.Clone();
            
            stopWatch.Start();
            
            for (int i = 0; i < maxLoop; i++)
            {
                var random = new Random();
                var maskList = new List<OpenCvSharp.Rect>();
                for (int j = 0; j < maxLoop/10; j++)
                {
                    var mask = new OpenCvSharp.Rect(random.Next(0, clone.Width - 1), random.Next(0, clone.Height - 1), random.Next(1, clone.Width), random.Next(1, clone.Height));
                    maskList.Add(mask);
                }

                using var mat = BitmapConverter.ToMat(clone);
                using Mat maskMat = new Mat(new Size(mat.Width, mat.Height), MatType.CV_8UC4, Scalar.Black);
                foreach (var mask in maskList)
                {
                    Cv2.Rectangle(maskMat, mask, Scalar.White, thickness: -1);
                }
                
                Cv2.BitwiseAnd(mat, maskMat, mat);
                
                mat.Dispose();
            }
            
            stopWatch.Stop();
            
            clone.Dispose();

            return stopWatch;
        }

        public void Dispose()
        {
            source?.Dispose();
        }
    }
}
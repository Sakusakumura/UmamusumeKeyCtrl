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
using Point = OpenCvSharp.Point;
using Range = OpenCvSharp.Range;
using Rect = OpenCvSharp.Rect;
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
                
                var random = new Random();

                var randomNums = new List<List<int>>();
                var min = Math.Min(source.Width, source.Height);
                var maskAreas = new List<Rect>();
                for (int j = 0; j < 9; j++)
                {
                    randomNums.Add(new List<int>());
                    for (int k = 0; k < 4; k++)
                    {
                        randomNums[j].Add(random.Next(0, min));
                    }
                    
                    maskAreas.Add(new Rect(randomNums[j][0], randomNums[j][1], randomNums[j][2], randomNums[j][3]));
                }

                var doPerformScalingResult = Do1(maxLoop, maskAreas);
                stopwatches1.Add(doPerformScalingResult);

                Debug.Print($"Task #1 end. Elapsed time: {doPerformScalingResult.ElapsedMilliseconds}");
            
                Debug.Print("Task #2 start.");

                var doPerformAffinResult = Do2(maxLoop, maskAreas);
                stopwatches2.Add(doPerformAffinResult);

                Debug.Print($"Task #2 end. Elapsed time: {doPerformAffinResult.ElapsedMilliseconds}");
            }

            List<(string, long, long)> results = new();
            
            Debug.Print("Result:");
            Debug.Print("No. | #1 | #2");
            
            for (int i = 0; i < sampleCount; i++)
            {
                Debug.Print($"#{i} | {stopwatches1[i].ElapsedMilliseconds} | {stopwatches2[i].ElapsedMilliseconds}");
            }

            Debug.Print("---------------------------------------------------------");
            
            Debug.Print($"Average:");
            
            Debug.Print("#1 | #2");
            
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

        private Stopwatch Do1(int maxLoop, List<Rect> maskAreas)
        {
            var stopWatch = new Stopwatch();

            var sourceMat = BitmapConverter.ToMat(source);
            var list = new List<Mat>(){sourceMat};

            stopWatch.Start();
            
            for (int j = 0; j < maxLoop; j++)
            {
                var mask = new Mat();
                mask.ConvertTo(mask, MatType.CV_8UC4);
                Cv2.Rectangle(mask, new Point(0, 0), new Point(sourceMat.Cols, sourceMat.Rows), Scalar.Black, thickness: -1);
                foreach (var maskArea in maskAreas)
                {
                    Cv2.Rectangle(mask, maskArea, Scalar.White, thickness: -1);
                }
                list.Add(mask);
            }

            stopWatch.Stop();
            list.ForEach(val => val.Dispose());

            return stopWatch;
        }
        
        private Stopwatch Do2(int maxLoop, List<Rect> maskAreas)
        {
            var stopWatch = new Stopwatch();

            var sourceMat = BitmapConverter.ToMat(source);
            var list = new List<Mat>(){sourceMat};

            stopWatch.Start();
            
            for (int j = 0; j < maxLoop; j++)
            {
                var mask = new Mat(sourceMat.Size(), MatType.CV_8UC4, Scalar.Black);
                
                foreach (var maskArea in maskAreas)
                {
                    Cv2.Rectangle(mask, maskArea, Scalar.White, thickness: -1);
                }
                list.Add(mask);
            }

            stopWatch.Stop();
            list.ForEach(val => val.Dispose());

            return stopWatch;
        }

        public void Dispose()
        {
            source?.Dispose();
        }
    }
}
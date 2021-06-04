using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Documents;
using umamusumeKeyCtl.Util;

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

                var doPerformScalingResult = DoPerformScaling(maxLoop);
                stopwatches1.Add(doPerformScalingResult);

                Debug.Print($"Task #1 end. Elapsed time: {doPerformScalingResult.ElapsedMilliseconds}");
            
                Debug.Print("Task #2 start.");

                var doPerformAffinResult = DoPerformAffin(maxLoop);
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

        private Stopwatch DoPerformScaling(int maxLoop)
        {
            var stopWatch = new Stopwatch();

            var list = new List<Bitmap>();
            
            stopWatch.Start();

            for (int i = 0; i < maxLoop; i++)
            {
                var clone = (Bitmap) source.Clone();
                //list.Add(clone.PerformScaling(100));
            }
            
            stopWatch.Stop();

            var count = list.Count;

            for (int i = 0; i < count; i++)
            {
                list[i].Dispose();
            }

            return stopWatch;
        }
        
        private Stopwatch DoPerformAffin(int maxLoop)
        {
            var stopWatch = new Stopwatch();

            var list = new List<Bitmap>();
            
            stopWatch.Start();

            for (int i = 0; i < maxLoop; i++)
            {
                var clone = (Bitmap) source.Clone();
                list.Add(clone.PerformScale(100));
            }
            
            stopWatch.Stop();
            
            var count = list.Count;

            for (int i = 0; i < count; i++)
            {
                list[i].Dispose();
            }

            return stopWatch;
        }

        public void Dispose()
        {
            source?.Dispose();
        }
    }
}
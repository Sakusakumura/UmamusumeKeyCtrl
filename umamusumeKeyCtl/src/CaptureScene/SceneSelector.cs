using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using umamusumeKeyCtl.ImageSimilarity.Factory;
using umamusumeKeyCtl.ImageSimilarity.Method;
using umamusumeKeyCtl.Properties;
using Point = OpenCvSharp.Point;
using Range = OpenCvSharp.Range;
using Size = OpenCvSharp.Size;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneSelector
    {
        public bool IsDebugMode { get; set; }
        private bool _isBusy;
        public event EventHandler<List<MatchingResult>> OnGetMatchingResults;
        public event EventHandler<Scene> SceneSelected; 
        public event EventHandler<Mat> ResultPrinted; 
        public event EventHandler<Mat> SrcTgtImgPrinted;

        public SceneSelector()
        {
            this.IsDebugMode = Settings.Default.IsDebugMode;
        }

        public async Task SelectScene(Bitmap capturedImage)
        {
            if (_isBusy)
            {
                return;
            }

            _isBusy = true;
            
            try
            {
                var sceneSettings = SceneHolder.Instance.Scenes.Select(val => (val.Setting, FeaturePoints: val.ScrappedImage.FeaturePointsInfo)).ToList();

                var matchingResults = await GetMatchingResults(capturedImage, sceneSettings);

                var succeeds = matchingResults.Where(val => val.Result);
                
                var scenes = SceneHolder.Instance.Scenes.ToList();
                
                if (IsDebugMode)
                {
                    var sceneList = SceneHolder.Instance.Scenes.ToList();
                    var mat = BitmapConverter.ToMat(capturedImage);
                    
                    
                    var printSrcTgtImgTask = Task.Run(() => PrintSrcTgtImg(sceneList, mat));
                    var printMatchingResultTask = Task.Run(() => PrintMatchingResult(sceneList, mat, matchingResults.ToList()));

                    _ = Task.Run(async () =>
                    {
                        await Task.WhenAll(printSrcTgtImgTask, printMatchingResultTask);
                        mat.Dispose();
                    });
                }

                if (succeeds.Count() > 0)
                {
                    var targetScene = scenes.Find(val => val.Setting.Guid == succeeds.First().SceneGuid);

                    scenes.Remove(targetScene);
                    targetScene.IsSelected = true;
                    SceneSelected?.Invoke(this, targetScene);
                }
                else
                {
                    var targetScene = scenes.Find(val => val.Setting.DisplayName == "Default");

                    if (targetScene != null)
                    {
                        var defaultMResult = new MatchingResult(true, 0, new (), targetScene.Setting.DisplayName);
                        matchingResults.Add(defaultMResult);
                        
                        scenes.Remove(targetScene);
                        targetScene.IsSelected = true;
                        SceneSelected?.Invoke(this, targetScene);
                    }
                }

                foreach (var scene in scenes)
                {
                    scene.IsSelected = false;
                }

                if (IsDebugMode)
                {
                    OnGetMatchingResults?.Invoke(this, matchingResults);
                }
            }
            catch (Exception e)
            {
                Debug.Write(e);
                throw;
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async Task<List<MatchingResult>> GetMatchingResults(Bitmap capturedImage, IEnumerable<(SceneSetting Setting, DetectAndComputeResult FeaturePoints)> scenes)
        {
            try
            {
                // Exclude Default scene.
                var targetInfos = scenes.Where(val => val.Setting.DisplayName != "Default");

                // Convert capturedImage to mat.
                using var cloned = ((Bitmap) capturedImage.Clone());
                var mat = BitmapConverter.ToMat(cloned);

                // Create Task.
                var rootTask = new List<Task<MatchingResult>>();

                foreach (var info in targetInfos)
                {
                    rootTask.Add(GetMatchingResult(mat, info.Setting, info.FeaturePoints));
                }

                // Run task.
                var resultsList = await Task<List<MatchingResult>>.WhenAll(rootTask);

                // Sorting list.
                var returns = resultsList.OrderByDescending(val => val.Result).ThenBy(val => val.Score);

                // Dispose Object.
                mat.Dispose();

                return returns.ToList();
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                throw;
            }
        }

        private async Task<MatchingResult> GetMatchingResult(Mat capturedImageMat, SceneSetting sceneSetting, DetectAndComputeResult featurePoint)
        {
            return await Task<MatchingResult>.Run(() =>
            {
                var maskAreas = sceneSetting.ScrapSetting.ScrapInfos.Select(val => val.ScrapArea.ToOpenCvRect());
                
                using var mask = new Mat(capturedImageMat.Size(), MatType.CV_8UC4, Scalar.Black);
                
                foreach (var maskArea in maskAreas)
                {
                    Cv2.Rectangle(mask, maskArea, Scalar.White, thickness: -1);
                }
                
                Cv2.BitwiseAnd(capturedImageMat, mask, mask);

                var imageSimilaritySearcher = new ImageSimilaritySearcher(sceneSetting.DetectorMethod, sceneSetting.DescriptorMethod);

                var detectAndCompeteResult = imageSimilaritySearcher.DetectAndCompete(mask);

                var matchingResult = imageSimilaritySearcher.KnnMatch(featurePoint, detectAndCompeteResult);

                return matchingResult.WithSceneName(sceneSetting.DisplayName).WithSceneGuid(sceneSetting.Guid);
            });
        }

        /// <summary>
        /// Debug: Print Source and Target image.
        /// </summary>
        /// <param name="sceneList">List of Scene instance.</param>
        /// <param name="sourceMat">The OpenCvSharp.Mat that was converted from captured image.</param>
        private void PrintSrcTgtImg(List<Scene> sceneList, Mat sourceMat)
        {
            try
            {
                // Define size.
                var tgtSize = sourceMat.Size();
            
                var srcSize = sourceMat.Size();
                srcSize.Width /= 3;
                srcSize.Height /= 3;
            
                var rootSize = new Size(tgtSize.Width + srcSize.Width * Math.Ceiling(sceneList.Count / 3.0) + 1, tgtSize.Height + 1);
            
                // Draw Images to rootMat
                Mat rootMat = new Mat(rootSize, MatType.CV_8UC4, Scalar.Black);
                using Mat tgtMat = sourceMat.Clone();
                var keyPoints = new List<KeyPoint>();
                for (int i = 0; i < sceneList.Count; i++)
                {
                    // Draw Source KeyPoints.
                    var scene = sceneList[i];
                    using Mat srcMat = BitmapConverter.ToMat(scene.ScrappedImage.Image);
                    Cv2.DrawKeypoints(srcMat, scene.ScrappedImage.FeaturePointsInfo.KeyPoints, srcMat, Scalar.Green);
                    Cv2.Resize(srcMat, srcMat, srcSize);

                    // Copy only srcMat to root.
                    Cv2.Rectangle(srcMat, new Rect(0, 0, srcMat.Width, srcMat.Height), Scalar.Green);
                    var colEnd = tgtSize.Width + ((int) Math.Floor(i / 3.0) + 1) * srcSize.Width;
                    var colStart = tgtSize.Width + (int) Math.Floor(i / 3.0) * srcSize.Width;
                    Cv2.CopyTo(srcMat, 
                        rootMat
                            .RowRange((i % 3) * srcSize.Height, (i % 3 + 1) * srcSize.Height)
                            .ColRange(colStart, colEnd));
                
                    // Draw Target KeyPoints.
                    var targetMask = scene.ScrappedImage.SamplingAreas.Select(val => val.ScrapArea.ToOpenCvRect()).ToArray();
                    using var mask = new Mat(sourceMat.Size(), MatType.CV_8UC4, Scalar.Black);
                    foreach (var maskArea in targetMask)
                    {
                        Cv2.Rectangle(mask, maskArea, Scalar.White, thickness: -1);
                    }
                    using var masked = sourceMat.BitwiseAnd(mask);
                    
                    using var detectAndCompeteResult =
                        new ImageSimilaritySearcher(scene.Setting.DetectorMethod, scene.Setting.DescriptorMethod).DetectAndCompete(masked);
                    keyPoints.AddRange(detectAndCompeteResult.KeyPoints);
                }
                
                Cv2.DrawKeypoints(tgtMat, keyPoints, tgtMat, Scalar.Aquamarine);
                Cv2.Rectangle(tgtMat, new Rect(0, 0, tgtMat.Width, tgtMat.Height), Scalar.Green);
                
                // Copy targetMat to root
                tgtMat.CopyTo(rootMat
                    .RowRange(0, tgtSize.Height)
                    .ColRange(0, tgtSize.Width));
            
                SrcTgtImgPrinted?.Invoke(this, rootMat);
            }
            catch (Exception e)
            {
                Debug.Print(sceneList.Count.ToString());
                Debug.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Debug: Print matching result
        /// </summary>
        /// <param name="sceneList"></param>
        /// <param name="sourceMat"></param>
        /// <param name="results"></param>
        private void PrintMatchingResult(List<Scene> sceneList, Mat sourceMat, List<MatchingResult> results)
        {
            try
            {
                if (results.Any(val => val.Result) == false || sourceMat.Cols / sourceMat.Rows == 1)
                {
                    return;
                }
            
                var succeeds = results.Where(val => val.Result);
                
                var rootHeight = sourceMat.Height * (2 - succeeds.Count() % 2);
                var rootWidth = (int) (sourceMat.Width * Math.Ceiling(succeeds.Count() / 2.0));
                var rootMatSize = new Size(rootWidth, rootHeight);

                using Mat rootMat = new Mat(rootMatSize, MatType.CV_8UC4, Scalar.Black);
                int i = 0;
                foreach (var matchingResult in succeeds)
                {
                    if (!sceneList.Exists(val => val.Setting.Guid == matchingResult.SceneGuid))
                    {
                        continue;
                    }
                    
                    var scene = sceneList.First(val => val.Setting.Guid == matchingResult.SceneGuid);
                    
                    var targetMask = scene.ScrappedImage.SamplingAreas.Select(val => val.ScrapArea.ToOpenCvRect()).ToArray();
                    using var mask = new Mat(sourceMat.Size(), MatType.CV_8UC4, Scalar.Black);
                    foreach (var maskArea in targetMask)
                    {
                        Cv2.Rectangle(mask, maskArea, Scalar.White, thickness: -1);
                    }
                    using var masked = sourceMat.BitwiseAnd(mask);
                    
                    var targetKeyPoints =
                        new ImageSimilaritySearcher(scene.Setting.DetectorMethod, scene.Setting.DescriptorMethod).DetectAndCompete(masked);
                    var sourceKeyPoints = scene.ScrappedImage.FeaturePointsInfo.KeyPoints;
                    var result = results[i];

                    using var srcMat = BitmapConverter.ToMat(scene.ScrappedImage.Image);
                    using var tgtMat = sourceMat.Clone();
                    using Mat matchesDrawedMat = new Mat();
                
                    try
                    {
                        if (sourceKeyPoints.Length > 0 && targetKeyPoints.KeyPoints.Length > 0)
                        {
                            // Draw matches.
                            Cv2.DrawMatchesKnn(
                                srcMat, sourceKeyPoints, 
                                tgtMat, targetKeyPoints.KeyPoints, 
                                result.KnnMatches, matchesDrawedMat, Scalar.Green, flags: DrawMatchesFlags.Default);
                            Cv2.Resize(matchesDrawedMat, matchesDrawedMat, new Size(matchesDrawedMat.Cols / 2, matchesDrawedMat.Rows / 2));
                            Cv2.Rectangle(matchesDrawedMat, new Rect(0, 0, matchesDrawedMat.Cols, matchesDrawedMat.Rows), Scalar.Green);

                            // Copy to root.
                            var rowRange = new Range((i % 2) * matchesDrawedMat.Rows, (i % 2 + 1) * matchesDrawedMat.Rows);
                            var colRange = new Range((int) Math.Floor(i / 2.0) * matchesDrawedMat.Cols, ((int) Math.Floor(i / 2.0) + 1) * matchesDrawedMat.Cols);
                            Debug.Print($"[{this.GetType().Name}] succeeds={succeeds.Count()}, rowRange.Start={rowRange.Start}, rowRange.End={rowRange.End}, rootMat.rows={rootMat.Rows}");
                            matchesDrawedMat.CopyTo(rootMat
                                .RowRange(rowRange)
                                .ColRange(colRange));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Print(e.ToString());
                    }
                    finally
                    {
                        i += 1;
                    }
                }

                ResultPrinted?.Invoke(this, rootMat);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }
    }
}
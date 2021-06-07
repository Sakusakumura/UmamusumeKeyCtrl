using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using umamusumeKeyCtl.Factory;
using umamusumeKeyCtl.FeaturePointMethod;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneSelector
    {
        private bool _printResult;
        private bool _isBusy;
        public event Action<List<MatchingResult>> OnGetMatchingResults;
        public event EventHandler<Scene> SceneSelected; 
        public event Action<Mat> ResultPrinted; 
        public event Action<(Mat Src, Mat Tgt)> SrcTgtImgPrinted;

        public SceneSelector(bool printResult)
        {
            this._printResult = printResult;
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
                var scenes = SceneHolder.Instance.Scenes.ToList();
                
                var matchingResults = await GetMatchingResults(capturedImage, scenes);

                var succeeds = matchingResults.Where(val => val.Result);

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
                        var defaultMResult = new MatchingResult(true, 0);
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

                OnGetMatchingResults?.Invoke(matchingResults);
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                throw;
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async Task<List<MatchingResult>> GetMatchingResults(Bitmap capturedImage, List<Scene> scenes)
        {
            try
            {
                var rootTask = new List<Task<MatchingResult>>();

                // Create task that gets matching result.
                scenes = scenes.Where(val => val.Setting.DisplayName != "Default").ToList();
                
                foreach (var scene in scenes)
                {
                    var cloned = ((Bitmap) capturedImage.Clone());

                    if (_printResult && scene.Setting.DisplayName == "育成/トレーニング１")
                    {
                        using Mat src = BitmapConverter.ToMat(scene.ScrappedImage.Image);
                        Cv2.DrawKeypoints(src, scene.ScrappedImage.FeaturePoints.KeyPoints, src, Scalar.Green);
                        var targetMask = scene.ScrappedImage.SamplingAreas.Select(val => val.ScrapArea.ToOpenCvRect()).ToArray();
                        var detectAndCompeteResult = ImageSimilaritySearcher.DetectAndCompete(cloned, MatchingFeaturePointMethod.ORB, targetMask);
                        using Mat tgt = BitmapConverter.ToMat(cloned);
                        Cv2.DrawKeypoints(tgt, detectAndCompeteResult.KeyPoints, tgt, Scalar.Green);
                        SrcTgtImgPrinted?.Invoke(new (src, tgt));
                    }


                    rootTask.Add(GetMatchingResult(cloned, scene, scene.ScrappedImage.FeaturePoints));
                }

                // Run task.
                var resultsList = await Task<List<MatchingResult>>.WhenAll(rootTask.ToArray());

                // Sorting list.
                var returns = resultsList.OrderByDescending(val => val.Result).ThenBy(val => val.Score).ToList();
                
                if (false && _printResult)
                {
                    var str = "Matches count | Result | SceneName\n";
                    foreach (var matchingResult in returns)
                    {
                        str += $"{matchingResult.Matches?.Length} | {matchingResult.Result} | {matchingResult.SceneName}\n";
                    }
                    Debug.Print(str);
                }
                
                // For debug.
                if (_printResult && returns.Exists(val => val.Result))
                {
                    using var cloned = (Bitmap) capturedImage.Clone();
                    
                    var scene = scenes.ToList().Find(val => val.Setting.Guid == returns.First().SceneGuid);
                    
                    var tgtResult = ImageSimilaritySearcher.DetectAndCompete(cloned, MatchingFeaturePointMethod.ORB,
                        scene.ScrappedImage.Setting.ScrapInfos.Select(val => val.ScrapArea.ToOpenCvRect()).ToArray());
                    var srcResult = scene.ScrappedImage.FeaturePoints.KeyPoints;
                    var result = returns.First();

                    using var srcMat = BitmapConverter.ToMat(scene.ScrappedImage.Image);
                    using var tgtMat = BitmapConverter.ToMat(cloned);
                    using Mat output = new Mat();
                    try
                    {
                        if (srcResult.Length > 0 && tgtResult.KeyPoints.Length > 0 )
                        {
                            Cv2.DrawMatches(srcMat,
                                srcResult,
                                tgtMat,
                                tgtResult.KeyPoints,
                                result.Matches,
                                output,
                                matchColor: Scalar.Green,
                                flags: DrawMatchesFlags.Default);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Print(e.ToString());
                    }

                    ResultPrinted?.Invoke(output);
                }

                return returns.ToList();
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                throw;
            }
        }

        private async Task<MatchingResult> GetMatchingResult(Bitmap capturedImage, Scene scene, DetectAndCompeteResult featurePoint)
        {
            return await Task<MatchingResult>.Run(() =>
            {
                var maskArea = scene.ScrappedImage.SamplingAreas.Select(val => val.ScrapArea.ToOpenCvRect()).ToArray();

                var matchingResult = ImageSimilaritySearcher.FeaturePointsMatching(featurePoint, capturedImage, MatchingFeaturePointMethod.ORB, maskArea);

                _ = Task.Run(() =>
                {
                    capturedImage.Dispose();
                });

                return matchingResult.WithSceneName(scene.Setting.DisplayName).WithSceneGuid(scene.Setting.Guid);
            });
        }
    }
}
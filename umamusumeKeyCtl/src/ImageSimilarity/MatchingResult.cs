using System;
using System.Collections.Generic;
using OpenCvSharp;

namespace umamusumeKeyCtl
{
    public class MatchingResult
    {
        public bool Result { get; set; }
        public double Score { get; set; }
        public Guid SceneGuid { get; set; }
        public string SceneName { get; set; }
        public List<DMatch> Matches { get; set; }
        public DMatch[][] KnnMatches { get; set; }

        /// <summary>
        /// Result of similarity search.
        /// </summary>
        /// <param name="result">Presents match or not.</param>
        /// <param name="score">Sum of DMatch.Distance. Lower score is good.</param>
        public MatchingResult(bool result, double score, string sceneName = "")
        {
            this.Result = result;
            this.Score = score;
            this.SceneName = sceneName;
            KnnMatches = new DMatch[][] { };
        }
        
        public MatchingResult(bool result, double score, List<DMatch> matches, string sceneName = "")
        {
            this.Result = result;
            this.Score = score;
            this.Matches = matches;
            this.SceneName = sceneName;
            KnnMatches = new DMatch[][] { };
        }

        public static MatchingResult FailWithScore(List<DMatch> matches)
        {
            return new MatchingResult(false, -1.0, matches);
        }

        public static MatchingResult SuccessWithScore(List<DMatch> matches)
        {
            var score = 0.0d;

            foreach (var match in matches)
            {
                score += match.Distance;
            }

            score /= matches.Count;
            // var log = Math.Log10(matches.Count + 1);
            // var k = (1 - 0.40369440861835 * log);
            //
            // score = Math.Max(0, score * k);

            return new MatchingResult(true, score, matches);
        }

        public MatchingResult WithSceneName(string name)
        {
            this.SceneName = name;
            return this;
        }

        public MatchingResult WithSceneGuid(Guid guid)
        {
            this.SceneGuid = guid;
            return this;
        }

        public MatchingResult WithKnnMatchResult(DMatch[][] matches)
        {
            KnnMatches = matches;
            return this;
        }
    }
}
using System;
using OpenCvSharp;

namespace umamusumeKeyCtl
{
    public class MatchingResult
    {
        public bool Result { get; }
        public double Score { get; }
        public string SceneName { get; private set; }
        public DMatch[] Matches { get; }

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
        }
        
        public MatchingResult(bool result, double score, DMatch[] matches, string sceneName = "")
        {
            this.Result = result;
            this.Score = score;
            this.Matches = matches;
            this.SceneName = sceneName;
        }

        public static MatchingResult Fail => new MatchingResult(false, -1.0);
        
        public static MatchingResult SuccessWithScore(DMatch[] matches)
        {
            var score = 0.0d;

            foreach (var match in matches)
            {
                score += match.Distance;
            }

            score /= matches.Length;

            return new MatchingResult(true, score, matches);
        }

        public MatchingResult WithSceneName(string name)
        {
            this.SceneName = name;
            return this;
        }
    }
}
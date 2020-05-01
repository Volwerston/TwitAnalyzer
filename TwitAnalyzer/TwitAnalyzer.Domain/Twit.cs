using System;

namespace TwitAnalyzer.Domain
{
    public enum CategorizationResult
    {
        Negative,
        Undetermined,
        Positive
    }

    public class Twit
    {
        public string Text { get; set; }
    }

    public class TwitAnalyzerResult
    {
        public string Algorithm { get; set; }
        public TwitAnalysisResult TwitAnalysisResult { get; set; }
    }

    public class TwitAnalysisResult
    {
        public string Text { get; set; }
        public float PositiveProbability { get; set; }
        public CategorizationResult CategorizationResult { get; set; }
        public DateTime TimestampUtc { get; set; }

        public static CategorizationResult Categorize(float probability, float Tolerance = 0.05f)
        {
            if (probability <= .5f - Tolerance)
                return CategorizationResult.Negative;

            return probability >= .5 + Tolerance
                ? CategorizationResult.Positive
                : CategorizationResult.Undetermined;
        }
    }
}

using Microsoft.ML.Data;

namespace MachineLearningAnalyzer
{
    public class SentimentData
    {
        [ColumnName("Label")]
        public bool Positive { get; set; }
        public string Text { get; set; }
    }

    public class SentimentPrediction : SentimentData
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }
}

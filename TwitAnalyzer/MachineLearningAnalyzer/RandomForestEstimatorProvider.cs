using Microsoft.ML;

namespace MachineLearningAnalyzer
{
    public class RandomForestEstimatorProvider : IEstimatorProvider
    {
        public IEstimator<ITransformer> GetEstimator(MLContext mlContext)
            => mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
                .Append(mlContext.BinaryClassification.Trainers.FastForest());

        public string Name => "random_forest";
    }
}

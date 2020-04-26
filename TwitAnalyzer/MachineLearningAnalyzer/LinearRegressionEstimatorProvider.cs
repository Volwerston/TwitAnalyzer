using Microsoft.ML;

namespace MachineLearningAnalyzer
{
    public class LinearRegressionEstimatorProvider : IEstimatorProvider
    {
        public IEstimator<ITransformer> GetEstimator(MLContext mlContext)
            => mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

        public string Name => "regression";
    }
}

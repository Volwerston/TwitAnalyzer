using Microsoft.ML;

namespace MachineLearningAnalyzer
{
    public interface IEstimatorProvider
    {
       IEstimator<ITransformer> GetEstimator(MLContext mlContext);
       string Name { get; }
    }
}

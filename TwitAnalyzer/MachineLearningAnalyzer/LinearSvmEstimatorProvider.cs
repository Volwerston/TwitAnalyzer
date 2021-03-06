﻿using Microsoft.ML;

namespace MachineLearningAnalyzer
{
    public class LinearSvmEstimatorProvider : IEstimatorProvider
    {
        public IEstimator<ITransformer> GetEstimator(MLContext mlContext)
            => mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
                .Append(mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression());

        public string Name => "svm";
    }
}
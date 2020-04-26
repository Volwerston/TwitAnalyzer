using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ML;
using TwitAnalyzer.Domain;

namespace MachineLearningAnalyzer
{
    public class MachineLearningAnalyzer : ITwitAnalyzer
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;

        internal MachineLearningAnalyzer(MLContext mlContext, ITransformer model)
        {
            _mlContext = mlContext;
            _model = model;
        }

        public static MachineLearningAnalyzer GetTrained(string dataSetPath)
        {
            var mlContext = new MLContext();

            var testData = LoadTestData(dataSetPath);

            var dataView = mlContext.Data.LoadFromEnumerable(testData);

            var model = BuildAndTrainModel(mlContext, dataView);

            return new MachineLearningAnalyzer(mlContext, model);
        }

        public Task<TwitAnalyzerResult> Analyze(Twit twit)
        {
            var predictionFunction = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);

            var sampleStatement = new SentimentData
            {
                Text = twit.Text
            };

            var prediction = predictionFunction.Predict(sampleStatement);

            return Task.FromResult(
                new TwitAnalyzerResult
                {
                    Algorithm = "ml",
                    TwitAnalysisResult = new TwitAnalysisResult
                    {
                        CategorizationResult = TwitAnalysisResult.Categorize(prediction.Probability),
                        PositiveProbability = prediction.Probability,
                        Text = twit.Text
                    }
                });
        }

        private static ITransformer BuildAndTrainModel(MLContext mlContext, IDataView splitTrainSet)
        {
            var estimator = mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

            var model = estimator.Fit(splitTrainSet);

            return model;
        }

        private static IReadOnlyCollection<SentimentData> LoadTestData(string dataSetPath)
        {
            var testData = new List<SentimentData>();

            using (var fs = new FileStream(dataSetPath, FileMode.Open))
            {
                using (var sr = new StreamReader(fs))
                {
                    sr.ReadLine();

                    string line = null;

                    while ((line = sr.ReadLine()) != null)
                    {
                        var data = line.Split(new[] { ',' });

                        switch (data[0].Trim())
                        {
                            case "Pos":
                                testData.Add(new SentimentData
                                {
                                    Positive = true,
                                    Text = data[1].Trim()
                                });
                                break;
                            case "Neg":
                                testData.Add(new SentimentData
                                {
                                    Positive = false,
                                    Text = data[1].Trim()
                                });
                                break;
                        }
                    }
                }
            }

            return testData;
        }
    }
}

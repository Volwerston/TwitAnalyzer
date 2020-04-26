using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ML;
using TwitAnalyzer.Domain;

namespace MachineLearningAnalyzer
{
    public class MachineLearningAnalyzer<TEstimatorProvider>: ITwitAnalyzer
        where TEstimatorProvider : IEstimatorProvider, new()
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;
        private readonly string _estimationAlgorithm;

        internal MachineLearningAnalyzer(MLContext mlContext, ITransformer model, string estimationAlgorithm)
        {
            _mlContext = mlContext;
            _model = model;
            _estimationAlgorithm = estimationAlgorithm;
        }

        public static MachineLearningAnalyzer<TEstimatorProvider> GetTrained(string dataSetPath)
        {
            var estimatorProvider = new TEstimatorProvider();

            var mlContext = new MLContext();

            var testData = LoadTestData(dataSetPath);

            var dataView = mlContext.Data.LoadFromEnumerable(testData);

            var model = BuildAndTrainModel(mlContext, dataView, estimatorProvider);

            return new MachineLearningAnalyzer<TEstimatorProvider>(mlContext, model, estimatorProvider.Name);
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
                    Algorithm = _estimationAlgorithm,
                    TwitAnalysisResult = new TwitAnalysisResult
                    {
                        CategorizationResult = TwitAnalysisResult.Categorize(prediction.Probability),
                        PositiveProbability = prediction.Probability,
                        Text = twit.Text
                    }
                });
        }

        private static ITransformer BuildAndTrainModel(MLContext mlContext, IDataView trainSet, IEstimatorProvider estimatorProvider)
        {
            var estimator = estimatorProvider.GetEstimator(mlContext);

            var model = estimator.Fit(trainSet);

            return model;
        }

        private static IReadOnlyCollection<SentimentData> LoadTestData(string dataSet)
        {
            var testData = new List<SentimentData>();

            using (var fs = GenerateStreamFromString(dataSet))
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

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}

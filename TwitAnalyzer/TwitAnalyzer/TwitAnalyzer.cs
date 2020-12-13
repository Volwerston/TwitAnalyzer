using System;
using System.IO;
using System.Threading.Tasks;
using MachineLearningAnalyzer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TwitAnalyzer.Application.Interfaces;
using TwitAnalyzer.Domain;
using TwitAnalyzer.Interfaces;
using nBayes;

namespace TwitAnalyzer
{
    public class TwitAnalyzer
    {
        private readonly ITwitIndexer _twitIndexer;
        private readonly IIndexerSettings _settings;
        private readonly BayesAnalyzer _bayesAnalyzer;
        private readonly MachineLearningAnalyzer<LinearRegressionEstimatorProvider> _linearRegressionAnalyzer;
        private readonly MachineLearningAnalyzer<RandomForestEstimatorProvider> _randomForestAnalyzer;

        public TwitAnalyzer(
            ITwitIndexer twitIndexer,
            IIndexerSettings settings,
            BayesAnalyzer bayesAnalyzer,
            MachineLearningAnalyzer<LinearRegressionEstimatorProvider> linearRegressionAnalyzer,
            MachineLearningAnalyzer<RandomForestEstimatorProvider> randomForestAnalyzer)
        {
            _twitIndexer = twitIndexer;
            _settings = settings;
            _bayesAnalyzer = bayesAnalyzer;
            _linearRegressionAnalyzer = linearRegressionAnalyzer;
            _randomForestAnalyzer = randomForestAnalyzer;
        }

        [FunctionName("TwitAnalyzer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger logger)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var twit = new Twit { Text = requestBody };

            var bayesAnalysisResult = await _bayesAnalyzer.Analyze(twit);
            logger.LogInformation($"Naive Bayes probability: '{bayesAnalysisResult.TwitAnalysisResult.PositiveProbability}'");

            await _twitIndexer.Index(
                bayesAnalysisResult.TwitAnalysisResult,
                GetIndexName(_settings.Tag, bayesAnalysisResult.Algorithm));

            var linearRegressionAnalysisResult = await _linearRegressionAnalyzer.Analyze(twit);
            logger.LogInformation($"Linear SVM probability: '{linearRegressionAnalysisResult.TwitAnalysisResult.PositiveProbability}'");

            await _twitIndexer.Index(
                linearRegressionAnalysisResult.TwitAnalysisResult,
                GetIndexName(_settings.Tag, linearRegressionAnalysisResult.Algorithm));

            var randomForestAnalysisResult = await _randomForestAnalyzer.Analyze(twit);
            logger.LogError($"Random forest probability: '{linearRegressionAnalysisResult.TwitAnalysisResult.PositiveProbability}'");

            await _twitIndexer.Index(
                randomForestAnalysisResult.TwitAnalysisResult,
                GetIndexName(_settings.Tag, randomForestAnalysisResult.Algorithm));

            return new OkObjectResult("Twit processed successfully");
        }

        private static string GetIndexName(string tag, string algorithm) => $"{DateTime.UtcNow:yyyy-MM-dd}_{algorithm}_{tag}";
    }
}

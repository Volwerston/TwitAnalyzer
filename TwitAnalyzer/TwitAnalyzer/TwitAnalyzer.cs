using System;
using System.IO;
using System.Threading.Tasks;
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
        private readonly MachineLearningAnalyzer.MachineLearningAnalyzer _machineLearningAnalyzer;

        public TwitAnalyzer(
            ITwitIndexer twitIndexer, 
            IIndexerSettings settings,
            BayesAnalyzer bayesAnalyzer,
            MachineLearningAnalyzer.MachineLearningAnalyzer mlAnalyzer)
        {
            _twitIndexer = twitIndexer;
            _settings = settings;
            _bayesAnalyzer = bayesAnalyzer;
            _machineLearningAnalyzer = mlAnalyzer;
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

            var machineLearningAnalysisResult = await _machineLearningAnalyzer.Analyze(twit);
            logger.LogInformation($"ML probability: '{machineLearningAnalysisResult.TwitAnalysisResult.PositiveProbability}'");

            await _twitIndexer.Index(
                machineLearningAnalysisResult.TwitAnalysisResult,
                GetIndexName(_settings.Tag, machineLearningAnalysisResult.Algorithm));

            return new OkObjectResult("Twit processed successfully");
        }

        private static string GetIndexName(string tag, string algorithm) => $"{DateTime.UtcNow:yyyy-MM-dd}_{algorithm}_{tag}";
    }
}

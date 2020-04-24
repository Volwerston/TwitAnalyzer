using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
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

        public TwitAnalyzer(
            ITwitIndexer twitIndexer, 
            IIndexerSettings settings,
            BayesAnalyzer bayesAnalyzer)
        {
            _twitIndexer = twitIndexer;
            _settings = settings;
            _bayesAnalyzer = bayesAnalyzer;
        }

        [FunctionName("TwitAnalyzer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            var twit = new Twit { Text = requestBody };

            var bayesAnalysisResult = await _bayesAnalyzer.Analyze(twit);

            await _twitIndexer.Index(
                bayesAnalysisResult.TwitAnalysisResult, 
                GetIndexName(_settings.Tag, bayesAnalysisResult.Algorithm));

            return new OkObjectResult("Twit processed successfully");
        }

        private static string GetIndexName(string tag, string algorithm) => $"{DateTime.UtcNow:yyyy-MM-dd}_{algorithm}_{tag}";
    }
}

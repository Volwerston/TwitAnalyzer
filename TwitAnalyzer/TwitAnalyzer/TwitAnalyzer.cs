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

namespace TwitAnalyzer
{
    public class TwitAnalyzer
    {
        private readonly ITwitIndexer _twitIndexer;
        private readonly IIndexerSettings _settings;

        public TwitAnalyzer(ITwitIndexer twitIndexer, IIndexerSettings settings)
        {
            _twitIndexer = twitIndexer;
            _settings = settings;
        }

        [FunctionName("TwitAnalyzer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var twit = new Twit { Text = requestBody };

            await _twitIndexer.Index(twit, GetIndexName(_settings.Tag));

            return new OkObjectResult("Twit processed successfully");
        }

        private static string GetIndexName(string tag) => $"{DateTime.UtcNow:yyyy-MM-dd}_{tag}";
    }
}

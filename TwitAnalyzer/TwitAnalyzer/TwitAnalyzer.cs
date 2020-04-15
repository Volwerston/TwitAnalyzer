using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using TwitAnalyzer.Application.Interfaces;
using TwitAnalyzer.Domain;

namespace TwitAnalyzer
{
    public class TwitAnalyzer
    {
        private readonly ITwitIndexer _twitIndexer;

        public TwitAnalyzer(ITwitIndexer twitIndexer)
        {
            _twitIndexer = twitIndexer;
        }

        [FunctionName("TwitAnalyzer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var twit = new Twit { Text = requestBody };

            await _twitIndexer.Index(twit, GetIndexName());

            return new OkObjectResult("Twit processed successfully");
        }

        private static string GetIndexName() => $"{DateTime.UtcNow:yyyy-MM-dd}_hey";
    }
}

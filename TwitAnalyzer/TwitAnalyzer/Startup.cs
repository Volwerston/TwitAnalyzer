using System;
using System.IO;
using Elasticsearch.Net;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nBayes;
using Nest;
using TwitAnalyzer.Application.Implementations;
using TwitAnalyzer.Application.Interfaces;
using TwitAnalyzer.Implementations;
using TwitAnalyzer.Interfaces;

[assembly: FunctionsStartup(typeof(TwitAnalyzer.Startup))]

namespace TwitAnalyzer
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton(sp =>
            {
                var nodes = new[]
                {
                    new Uri("http://localhost:9200"),
                };

                var pool = new StaticConnectionPool(nodes);
                var settings = new ConnectionSettings(pool);

                return new ElasticClient(settings);
            });

            builder.Services.AddSingleton<ITwitIndexer, ElasticTwitIndexer>();
            builder.Services.AddSingleton<IIndexerSettings>(sp => new Settings(GetConfiguration()));

            var bayesAnalyzer = BayesAnalyzer.GetTrained("bayes-dataset.csv");
            builder.Services.AddSingleton(bayesAnalyzer);

            var machineLearningAnalyzer = MachineLearningAnalyzer.MachineLearningAnalyzer.GetTrained("bayes-dataset.csv");
            builder.Services.AddSingleton(machineLearningAnalyzer);
        }

        private static IConfiguration GetConfiguration()
        {
            var appConfiguration = new ConfigurationBuilder()
#if DEBUG
                .SetBasePath(Directory.GetCurrentDirectory())
#else
                .SetBasePath(Environment.ExpandEnvironmentVariables(@"%home%\site\wwwroot"))
#endif
                .AddJsonFile("application.settings.json", false)
                .AddEnvironmentVariables()
                .Build();

            return appConfiguration;
        }
    }
}

using System;
using Elasticsearch.Net;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using TwitAnalyzer.Application.Implementations;
using TwitAnalyzer.Application.Interfaces;

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
        }
    }
}

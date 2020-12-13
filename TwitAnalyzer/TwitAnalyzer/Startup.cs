using System;
using System.IO;
using Elasticsearch.Net;
using MachineLearningAnalyzer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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
                    new Uri("http://40.115.48.238:9200"),
                };

                var pool = new StaticConnectionPool(nodes);

                var settings = new ConnectionSettings(pool);
                settings.BasicAuthentication("elastic", "MyPa$$w0rd123");

                return new ElasticClient(settings);
            });

            builder.Services.AddSingleton<ITwitIndexer, ElasticTwitIndexer>();
            builder.Services.AddSingleton<IIndexerSettings>(sp => new Settings(GetConfiguration()));

            var dataSetBlob = GetBlob("datasets", "bayes-dataset.csv");

            var bayesAnalyzer = BayesAnalyzer.GetTrained(dataSetBlob);
            builder.Services.AddSingleton(bayesAnalyzer);

            var linearSvmAnalyzer = MachineLearningAnalyzer<LinearSvmEstimatorProvider>
                .GetTrained(dataSetBlob);

            builder.Services.AddSingleton(linearSvmAnalyzer);

            var linearRegressionAnalyzer = MachineLearningAnalyzer<LinearRegressionEstimatorProvider>
                .GetTrained(dataSetBlob);

            builder.Services.AddSingleton(linearRegressionAnalyzer);

            var randomForestAnalyzer = MachineLearningAnalyzer<RandomForestEstimatorProvider>
                .GetTrained(dataSetBlob);

            builder.Services.AddSingleton(randomForestAnalyzer);
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

        public string GetBlob(string containerName, string fileName)
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=storageaccounttwitt9a52;AccountKey=jABrP9AbrYSyRvlJz8CDed9BHg9s/Pf7Nm161iTsikIEkayz9FNikcSakv+kW3nwFDohywbpakmx81EyO79jxg==;EndpointSuffix=core.windows.net";

            // Setup the connection to the storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Connect to the blob storage
            CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();
            // Connect to the blob container
            CloudBlobContainer container = serviceClient.GetContainerReference($"{containerName}");
            // Connect to the blob file
            CloudBlockBlob blob = container.GetBlockBlobReference($"{fileName}");
            // Get the blob file as text
            string contents = blob.DownloadTextAsync().Result;

            return contents;
        }
    }
}

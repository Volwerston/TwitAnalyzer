using System.Threading.Tasks;
using Nest;
using TwitAnalyzer.Application.Interfaces;
using TwitAnalyzer.Domain;

namespace TwitAnalyzer.Application.Implementations
{
    public class ElasticTwitIndexer : ITwitIndexer
    {
        private readonly ElasticClient _elasticClient;

        public ElasticTwitIndexer(ElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public Task Index(Twit twit, string index)
            => _elasticClient.IndexAsync(twit, idx => idx.Index(index));
    }
}

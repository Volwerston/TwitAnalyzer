using Microsoft.Extensions.Configuration;
using TwitAnalyzer.Interfaces;

namespace TwitAnalyzer.Implementations
{
    public class Settings : IIndexerSettings
    {
        private readonly IConfiguration _configuration;

        public Settings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Tag => _configuration.GetValue<string>("Tag");
    }
}

using System.Threading.Tasks;

namespace TwitAnalyzer.Domain
{
    public interface ITwitAnalyzer
    {
        Task<TwitAnalyzerResult> Analyze(Twit twit);
    }
}

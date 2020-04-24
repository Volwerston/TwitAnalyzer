using System.Threading.Tasks;
using TwitAnalyzer.Domain;

namespace TwitAnalyzer.Application.Interfaces
{
    public interface ITwitIndexer
    {
        Task Index(TwitAnalysisResult twit, string index);
    }
}

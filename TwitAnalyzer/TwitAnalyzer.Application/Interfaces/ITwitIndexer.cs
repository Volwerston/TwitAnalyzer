using System.Threading.Tasks;
using TwitAnalyzer.Domain;

namespace TwitAnalyzer.Application.Interfaces
{
    public interface ITwitIndexer
    {
        Task Index(Twit twit, string index);
    }
}

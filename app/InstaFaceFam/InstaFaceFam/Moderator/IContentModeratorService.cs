using System.Threading.Tasks;

namespace InstaFaceFam.Moderator
{
    public interface IContentModeratorService
    {
        Task<string> CreateTermList(string name, string description);
        Task AddTermToTermList(string termListId, string term);
        Task<ContentModeratorTextResults> ScreenTextAsync(string text, string termListId);
    }
}

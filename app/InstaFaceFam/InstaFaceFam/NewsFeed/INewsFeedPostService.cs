using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstaFaceFam.NewsFeed
{
    public interface INewsFeedPostService
    {
        Task CreateNewsFeedPostAsync(NewsFeedPost newsFeedPost);
        Task<ICollection<NewsFeedPost>> GetNewsFeedPostsAsync(string username);
    }
}

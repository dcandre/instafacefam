using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace InstaFaceFam.NewsFeed
{
    public class NewsFeedPostService : INewsFeedPostService
    {
        private readonly string _endpoint;
        private readonly string _key;
        private readonly string _databaseId;
        private readonly string _containerId;

        public NewsFeedPostService(string endpoint, string key, string databaseId, string containerId)
        {
            _endpoint = endpoint;
            _key = key;
            _databaseId = databaseId;
            _containerId = containerId;
        }

        public async Task CreateNewsFeedPostAsync(NewsFeedPost newsFeedPost)
        {
            try
            {
                using (var cosmosClient = new CosmosClient(_endpoint, _key))
                {
                    var cosmosContainer = cosmosClient.GetContainer(_databaseId, _containerId);
                    
                    var result = await cosmosContainer.CreateItemAsync(newsFeedPost, partitionKey: new PartitionKey(newsFeedPost.Username));
                }
            }
            catch(CosmosException e)
            {
                throw new Exception("The news feed post could not be uploaded.", e);
            }
        }

        public async Task<ICollection<NewsFeedPost>> GetNewsFeedPostsAsync(string username)
        {
            var newsFeedPosts = new List<NewsFeedPost>();

            try
            {
                using (var cosmosClient = new CosmosClient(_endpoint, _key))
                {
                    var cosmosContainer = cosmosClient.GetContainer(_databaseId, _containerId);

                    var newsFeedPostFeedIterator = cosmosContainer.GetItemLinqQueryable<NewsFeedPost>()
                        .Where(nfp => nfp.Username == username)
                        .OrderByDescending(nfp => nfp.PostedDate)
                        .ToFeedIterator();

                    while (newsFeedPostFeedIterator.HasMoreResults)
                    {
                        foreach(var newsFeedPost in await newsFeedPostFeedIterator.ReadNextAsync())
                        {
                            newsFeedPosts.Add(newsFeedPost);
                        }
                    }                    
                }
            }
            catch (CosmosException e)
            {
                throw new Exception("The news feed posts could not be retrieved.", e);
            }

            return newsFeedPosts;
        }
    }
}

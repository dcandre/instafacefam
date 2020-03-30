using System;
using InstaFaceFam.Moderator;
using InstaFaceFam.Vision;
using Newtonsoft.Json;

namespace InstaFaceFam.NewsFeed
{
    public class NewsFeedPost
    {
        [JsonProperty(PropertyName = "id")]
        public virtual string Id { get; set; }
        public string ImageUrl { get; set; }
        public string Message { get; set; }
        public ComputerVisionResults ImageVisionResults { get; set; }
        public ContentModeratorTextResults MessageModeratorResults { get; set; }
        public DateTimeOffset PostedDate { get; set; }
        public string Username { get; set; }
        public string Caption { get; set; }
        public string DescriptionTags { get; set; }
        public string Celebrities { get; set; }
        public string Landmarks { get; set; }
        public string Brands { get; set; }
    }
}

using System.IO;
using System.Threading.Tasks;

namespace InstaFaceFam.NewsFeed
{
    public interface ICustomerImageService
    {
        Task UploadCustomerImageAsync(string customerImageName, Stream stream, string contentType);
        string GetCustomerImageUrl(string imageName);
    }
}

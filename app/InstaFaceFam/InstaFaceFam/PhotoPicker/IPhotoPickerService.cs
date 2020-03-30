using System.IO;
using System.Threading.Tasks;

namespace InstaFaceFam.PhotoPicker
{
    public interface IPhotoPickerService
    {
        Task<Stream> GetImageStreamAsync(bool fromCamera);
    }
}

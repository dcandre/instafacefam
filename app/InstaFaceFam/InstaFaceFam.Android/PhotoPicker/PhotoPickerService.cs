using System.IO;
using System.Threading.Tasks;
using Android.Content;
using InstaFaceFam.Droid.PhotoPicker;
using InstaFaceFam.PhotoPicker;
using Xamarin.Forms;

[assembly: Dependency(typeof(PhotoPickerService))]
namespace InstaFaceFam.Droid.PhotoPicker
{
    public class PhotoPickerService : IPhotoPickerService
    {
        public Task<Stream> GetImageStreamAsync(bool fromCamera)
        {
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);

            MainActivity.Instance.StartActivityForResult(
            Intent.CreateChooser(intent, "Select Picture"),
            MainActivity.PickImageId);

            MainActivity.Instance.PickImageTaskCompletionSource = new TaskCompletionSource<Stream>();

            return MainActivity.Instance.PickImageTaskCompletionSource.Task;
        }
    }
}

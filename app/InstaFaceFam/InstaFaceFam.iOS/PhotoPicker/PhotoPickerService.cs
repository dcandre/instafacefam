using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using InstaFaceFam.iOS.PhotoPicker;
using InstaFaceFam.PhotoPicker;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PhotoPickerService))]
namespace InstaFaceFam.iOS.PhotoPicker
{
    public class PhotoPickerService : IPhotoPickerService
    {
        TaskCompletionSource<Stream> taskCompletionSource;
        UIImagePickerController imagePicker;

        public Task<Stream> GetImageStreamAsync(bool fromCamera)
        {
            if(fromCamera)
            {
                imagePicker = new UIImagePickerController
                {
                    SourceType = UIImagePickerControllerSourceType.Camera,
                    MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.Camera),
                    AllowsEditing = true
                };
            }
            else
            {
                imagePicker = new UIImagePickerController
                {
                    SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
                    MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary),
                    AllowsEditing = true
                };
            }            

            imagePicker.FinishedPickingMedia += OnImagePickerFinishedPickingMedia;
            imagePicker.Canceled += OnImagePickerCancelled;

            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            var viewController = window.RootViewController;
            viewController.PresentModalViewController(imagePicker, true);

            taskCompletionSource = new TaskCompletionSource<Stream>();
            return taskCompletionSource.Task;
        }

        private void OnImagePickerFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs args)
        {
            UIImage image = args.EditedImage ?? args.OriginalImage;

            if (image != null)
            {
                NSData data;

                if (args.ReferenceUrl != null &&
                    (args.ReferenceUrl.PathExtension.Equals("PNG") || args.ReferenceUrl.PathExtension.Equals("png")))
                {
                    data = image.AsPNG();
                }
                else
                {
                    data = image.AsJPEG(1);
                }

                Stream stream = data.AsStream();
               
                UnregisterEventHandlers();

                taskCompletionSource.SetResult(stream);
            }
            else
            {
                UnregisterEventHandlers();
                taskCompletionSource.SetResult(null);
            }

            imagePicker.DismissModalViewController(true);
        }

        private void OnImagePickerCancelled(object sender, EventArgs args)
        {
            UnregisterEventHandlers();
            taskCompletionSource.SetResult(null);
            imagePicker.DismissModalViewController(true);
        }

        private void UnregisterEventHandlers()
        {
            imagePicker.FinishedPickingMedia -= OnImagePickerFinishedPickingMedia;
            imagePicker.Canceled -= OnImagePickerCancelled;
        }
    }
}

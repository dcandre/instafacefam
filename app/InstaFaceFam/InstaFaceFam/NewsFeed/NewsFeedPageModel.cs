using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreshMvvm;
using InstaFaceFam.Moderator;
using InstaFaceFam.PhotoPicker;
using InstaFaceFam.Vision;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace InstaFaceFam.NewsFeed
{
    public class NewsFeedPageModel : FreshBasePageModel
    {
        private string _messageEditorText;
        public string MessageEditorText
        {
            get
            {
                return _messageEditorText;
            }
            set
            {
                _messageEditorText = value;
                RaisePropertyChanged("MessageEditorText");
            }
        }

        public string PhotoUploadButtonText { get; set; } = "Post Photo";

        public string MessageUploadButtonText { get; set; } = "Post Message";

        public string TermListId { get; set; } = "21";

        private bool _isActivityIndicatorRunning;
        public bool IsActivityIndicatorRunning
        {
            get
            {
                return _isActivityIndicatorRunning;
            }
            set
            {
                _isActivityIndicatorRunning = value;
                RaisePropertyChanged("IsActivityIndicatorRunning");
            }
        }

        public ObservableCollection<NewsFeedPost> NewsFeedPosts { get; private set; } = new ObservableCollection<NewsFeedPost>();

        private IComputerVisionService _computerVisionService;
        public IComputerVisionService ComputerVisionService
        {
            get
            {
                if(_computerVisionService == null)
                {
                    _computerVisionService = FreshIOC.Container.Resolve<IComputerVisionService>();
                }

                return _computerVisionService;
            }
        }

        private ICustomerImageService _customerImageService;
        public ICustomerImageService CustomerImageService
        {
            get
            {
                if (_customerImageService == null)
                {
                    _customerImageService = FreshIOC.Container.Resolve<ICustomerImageService>();
                }

                return _customerImageService;
            }
        }

        private IContentModeratorService _contentModeratorService;
        public IContentModeratorService ContentModeratorService
        {
            get
            {
                if (_contentModeratorService == null)
                {
                    _contentModeratorService = FreshIOC.Container.Resolve<IContentModeratorService>();
                }

                return _contentModeratorService;
            }
        }

        private INewsFeedPostService _newsFeedPostService;
        public INewsFeedPostService NewsFeedPostService
        {
            get
            {
                if (_newsFeedPostService == null)
                {
                    _newsFeedPostService = FreshIOC.Container.Resolve<INewsFeedPostService>();
                }

                return _newsFeedPostService;
            }
        }
                
        public Command RefreshViewRefreshingCommand
        {
            get
            {
                return new Command(async (object sender) => {
                    var refreshView = sender as RefreshView;
                    await LoadNewsFeedPosts();
                    refreshView.IsRefreshing = false;
                });
            }
        }

        public Command MessageEditorTextFocused
        {
            get
            {
                return new Command(() => {

                });
            }
        }

        public Command PostPhotoButtonClicked
        {
            get
            {
                return new Command(async () => {
                    try
                    {
                        IsActivityIndicatorRunning = true;

                        using (Stream stream = await DependencyService.Get<IPhotoPickerService>().GetImageStreamAsync(false))
                        {
                            if(stream != null)
                            {
                                var imageByteArray = ConvertStreamToByteArray(stream);

                                var newsFeedPost = new NewsFeedPost()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Username = "The User",
                                    PostedDate = DateTimeOffset.UtcNow,
                                    Message = MessageEditorText
                                };

                                using(var memoryStream = new MemoryStream(imageByteArray))
                                {
                                    await RunCognitiveServicesOnNewsFeedPostAsync(memoryStream, newsFeedPost);
                                }

                                using (var memoryStream = new MemoryStream(imageByteArray))
                                {
                                    await UploadCustomerImageToAzureStorageAsync(memoryStream, newsFeedPost);
                                }                    

                                await NewsFeedPostService.CreateNewsFeedPostAsync(newsFeedPost);
                            }                            
                        }

                        await LoadNewsFeedPosts();

                        MessageEditorText = null;
                    }
                    catch(Exception e)
                    {
                        await CurrentPage.DisplayAlert("Error", e.Message, "OK");
                    }
                    finally
                    {
                        IsActivityIndicatorRunning = false;
                    }                    
                });
            }
        }

        public Command PostMessageButtonClicked
        {
            get
            {
                return new Command(async () => {
                    try
                    {                        
                        IsActivityIndicatorRunning = true;

                        if(IsMessageEditorTextValid(MessageEditorText))
                        {
                            var newsFeedPost = new NewsFeedPost()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Username = "The User",
                                PostedDate = DateTimeOffset.UtcNow,
                                Message = MessageEditorText
                            };

                            await RunContentModeratorServicesOnNewsFeedPostAsync(newsFeedPost);

                            await NewsFeedPostService.CreateNewsFeedPostAsync(newsFeedPost);

                            await LoadNewsFeedPosts();

                            MessageEditorText = null;
                        }
                        else
                        {
                            await CurrentPage.DisplayAlert("Wait!", "You need to write a message", "OK");
                        }                        
                    }
                    catch (Exception e)
                    {
                        await CurrentPage.DisplayAlert("Error", e.Message, "OK");
                    }
                    finally
                    {
                        IsActivityIndicatorRunning = false;
                    }
                });
            }
        }

        public Command MoreButtonClicked
        {
            get
            {
                return new Command((object sender) => {
                    var stackLayout = sender as StackLayout;

                    stackLayout.IsVisible = !stackLayout.IsVisible;
                });
            }
        }
                
        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);

            Task.Run(async () => {
                await MainThread.InvokeOnMainThreadAsync(() => { IsActivityIndicatorRunning = true; });
                //If you want to create a custom term list uncomment this code and add terms to look out for.
                //var termListId = await ContentModeratorService
                //        .CreateTermList("InstaFaceFamTermList", "This is the term list for InstaFaceFam");                
                //await MainThread.InvokeOnMainThreadAsync(() => {
                //    TermListId = termListId;
                //});
                //await ContentModeratorService.AddTermToTermList(TermListId, "Fiddlesticks");
                await LoadNewsFeedPosts();
                await MainThread.InvokeOnMainThreadAsync(() => { IsActivityIndicatorRunning = false; });
            });
        }

        private byte[] ConvertStreamToByteArray(Stream stream)
        {
            using(var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private async Task LoadNewsFeedPosts()
        {
            try
            {
                var newsFeedPosts = await NewsFeedPostService.GetNewsFeedPostsAsync("The User");

                var newsFeedPostLastIndex = NewsFeedPosts.Count - 1;
                while (newsFeedPostLastIndex >= 0)
                {
                    NewsFeedPosts.RemoveAt(newsFeedPostLastIndex);
                    newsFeedPostLastIndex--;
                }

                foreach(var newsFeedPost in newsFeedPosts)
                {
                    NewsFeedPosts.Add(newsFeedPost);
                }
            }
            catch (Exception e)
            {
                await CurrentPage.DisplayAlert("Error", e.Message, "OK");
            }
        }

        private string GetCustomerImageName(string username, DateTime dateTime)
        {
            var customerImageName = new StringBuilder("customerimage-" + username + "-" + dateTime.ToShortDateString() + "-" + dateTime.ToLongTimeString() + ".jpg");

            customerImageName.Replace(" ", "-");
            customerImageName.Replace("/", "-");
            customerImageName.Replace(":", "-");

            return customerImageName.ToString();
        }

        private async Task UploadCustomerImageToAzureStorageAsync(Stream stream, NewsFeedPost newsFeedPost)
        {
            var customerImageName = GetCustomerImageName(newsFeedPost.Username, DateTime.UtcNow);
            await CustomerImageService.UploadCustomerImageAsync(customerImageName, stream, "image/jpeg");
            newsFeedPost.ImageUrl = CustomerImageService.GetCustomerImageUrl(customerImageName);
        }

        private async Task RunContentModeratorServicesOnNewsFeedPostAsync(NewsFeedPost newsFeedPost)
        {
            if (IsMessageEditorTextValid(newsFeedPost.Message))
            {
                var contentModeratorTextResults = await ContentModeratorService.ScreenTextAsync(newsFeedPost.Message, TermListId);

                ValidatePostMessage(contentModeratorTextResults);
                newsFeedPost.MessageModeratorResults = contentModeratorTextResults;
            }
        }

        private async Task RunCognitiveServicesOnNewsFeedPostAsync(Stream stream, NewsFeedPost newsFeedPost)
        {
            Task<ContentModeratorTextResults> contentModeratorTextTask = null;

            if (IsMessageEditorTextValid(newsFeedPost.Message))
            {
                contentModeratorTextTask = ContentModeratorService.ScreenTextAsync(newsFeedPost.Message, TermListId);
            }
            
            var computerVisionTask = ComputerVisionService.GetComputerVisionResultsAsync(stream);

            var tasks = new List<Task>()
            {
                computerVisionTask
            };

            if (contentModeratorTextTask != null)
            {
                tasks.Add(contentModeratorTextTask);
            }

            await Task.WhenAll(tasks);

            if (computerVisionTask.IsFaulted)
            {
                throw new Exception("Something went wrong with your image upload.  Please try again later.");
            }

            ValidatePostImage(computerVisionTask.Result);
            newsFeedPost.ImageVisionResults = computerVisionTask.Result;
            PopulateCaptionProperty(newsFeedPost);
            PopulateDescriptionTagsProperty(newsFeedPost);
            PopulateCelebritiesProperty(newsFeedPost);
            PopulateLandmarksProperty(newsFeedPost);
            PopulateBrandsProperty(newsFeedPost);

            if (contentModeratorTextTask != null)
            {
                if(contentModeratorTextTask.IsFaulted)
                {
                    throw new Exception("Something went wrong with your message post.  Please try again later.");
                }

                ValidatePostMessage(contentModeratorTextTask.Result);
                newsFeedPost.MessageModeratorResults = contentModeratorTextTask.Result;
            }
        }

        private void PopulateCaptionProperty(NewsFeedPost newsFeedPost)
        {
            if (newsFeedPost != null && newsFeedPost.ImageVisionResults != null && newsFeedPost.ImageVisionResults.Captions != null)
            {
                var captionBuilder = new StringBuilder();

                foreach (var caption in newsFeedPost.ImageVisionResults.Captions)
                {
                    captionBuilder.Append(caption.Name + ". ");
                }

                newsFeedPost.Caption = captionBuilder.ToString().TrimEnd();
            }
        }

        private void PopulateDescriptionTagsProperty(NewsFeedPost newsFeedPost)
        {
            if (newsFeedPost != null && newsFeedPost.ImageVisionResults != null
                && newsFeedPost.ImageVisionResults.DescriptionTags != null)
            {
                newsFeedPost.DescriptionTags = string.Join(", ", newsFeedPost.ImageVisionResults.DescriptionTags);
            }
        }

        private void PopulateCelebritiesProperty(NewsFeedPost newsFeedPost)
        {
            if (newsFeedPost != null && newsFeedPost.ImageVisionResults != null
                && newsFeedPost.ImageVisionResults.Celebrities != null)
            {
                var celebrityBuilder = new StringBuilder();

                foreach (var celebrity in newsFeedPost.ImageVisionResults.Celebrities)
                {
                    celebrityBuilder.Append(celebrity.Name + ". ");
                }

                newsFeedPost.Celebrities = celebrityBuilder.ToString().TrimEnd();
            }
        }

        private void PopulateLandmarksProperty(NewsFeedPost newsFeedPost)
        {
            if (newsFeedPost != null && newsFeedPost.ImageVisionResults != null
                && newsFeedPost.ImageVisionResults.Landmarks != null)
            {
                var landmarkBuilder = new StringBuilder();

                foreach (var landmark in newsFeedPost.ImageVisionResults.Landmarks)
                {
                    landmarkBuilder.Append(landmark.Name + ". ");
                }

                newsFeedPost.Landmarks = landmarkBuilder.ToString().TrimEnd();
            }
        }

        private void PopulateBrandsProperty(NewsFeedPost newsFeedPost)
        {
            if (newsFeedPost != null && newsFeedPost.ImageVisionResults != null
                && newsFeedPost.ImageVisionResults.Landmarks != null)
            {
                var brandBuilder = new StringBuilder();

                foreach (var brand in newsFeedPost.ImageVisionResults.Brands)
                {
                    brandBuilder.Append(brand.Name + ". ");
                }

                newsFeedPost.Brands = brandBuilder.ToString().TrimEnd();
            }
        }

        private bool IsMessageEditorTextValid(string messageEditorText)
        {
            if (!string.IsNullOrEmpty(messageEditorText))
            {
                return true;
            }

            return false;
        }

        private void ValidatePostImage(ComputerVisionResults computerVisionResults)
        {
            if(computerVisionResults.IsAdultContent && computerVisionResults.AdultConfidence > 0.5)
            {
                throw new Exception("This image is sexually explicit.  It cannot be posted");
            }
        }

        private void ValidatePostMessage(ContentModeratorTextResults contentModeratorTextResults)
        {            
            if (contentModeratorTextResults.SexuallyExplicitConfidence > 0.5)
            {
                throw new Exception("This message is sexually explicit.  It cannot be posted");
            }

            if (contentModeratorTextResults.DoesContainPii)
            {
                var ssnOrIpa = contentModeratorTextResults.Pii.FirstOrDefault(pii => pii.Label == "SSN" || pii.Label == "IPA");

                if(ssnOrIpa != null)
                {
                    throw new Exception("This message contains social security numbers or ip addresses.  It cannot be posted.");
                }
            }

            if (contentModeratorTextResults.DoesContainProfaneTerms)
            {
                throw new Exception("This message contains profane terms.  It cannot be posted.");
            }
        }
    }
}

using FreshMvvm;
using InstaFaceFam.Moderator;
using InstaFaceFam.NewsFeed;
using InstaFaceFam.Vision;
using Xamarin.Forms;

namespace InstaFaceFam
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            SetupIoc();

            MainPage = FreshPageModelResolver.ResolvePageModel<NewsFeedPageModel>();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private void SetupIoc()
        {
            var computerVisionEndpoint = "";
            var computerVisionKey = "";
            FreshIOC.Container.Register<IComputerVisionService>(new ComputerVisionService(computerVisionEndpoint, computerVisionKey));

            var storageConnectionString = "";
            var storageContainerName = "";
            var customerImageStorageUrl = "";
            FreshIOC.Container.Register<ICustomerImageService>(new CustomerImageService(storageConnectionString, storageContainerName, customerImageStorageUrl));

            var contentModeratorEndpoint = "";
            var contentModeratorKey = "";
            FreshIOC.Container.Register<IContentModeratorService>(new ContentModeratorService(contentModeratorEndpoint, contentModeratorKey));

            var cosmosDbEndpoint = "";
            var cosmosDbKey = "";
            var cosmosDbDatabaseId = "";
            var cosmosDbContainerId = "";
            FreshIOC.Container.Register<INewsFeedPostService>(new NewsFeedPostService(cosmosDbEndpoint, cosmosDbKey, cosmosDbDatabaseId, cosmosDbContainerId));
        }
    }
}

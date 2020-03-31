# instafacefam

You will need a Azure subscription to work with the instafacefam app.  In the App.xaml.cs file you will need to add your various endpoints, connection strings, database and container names, and keys for your storage service, Cosmos DB, computer vision service, and your content moderator service.

The instafacefam app uses the Cosmos DB client.  Right now there is a linker problem with Xamarin and this client.  I have implemented the fix for this in the iOS app.  You can check out an explanation for the fix [here.](https://github.com/Azure/azure-cosmos-dotnet-v3/issues/1243)  

The presentation that I created for this app uses reveal.js.  Just go into the presentation folder and open the index.html file in a web browser.  

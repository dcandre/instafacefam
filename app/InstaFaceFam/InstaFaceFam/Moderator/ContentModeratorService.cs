using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;

namespace InstaFaceFam.Moderator
{
    public class ContentModeratorService : IContentModeratorService
    {
        private readonly string _endpoint;
        private readonly string _key;

        public ContentModeratorService(string endpoint, string key)
        {
            _endpoint = endpoint;
            _key = key;
        }

        public async Task<string> CreateTermList(string name, string description)
        {
            using (var contentModeratorClient = new ContentModeratorClient(new ApiKeyServiceClientCredentials(_key)) { Endpoint = _endpoint })
            {
                var body = new Body(name, description);
                var termList = await contentModeratorClient.ListManagementTermLists.CreateAsync("application/json", body);

                if (!termList.Id.HasValue)
                {
                    throw new Exception("Your term list cannot be created right now.");
                }
                               
                return termList.Id.Value.ToString();
            }
        }

        public async Task AddTermToTermList(string termListId, string term)
        {
            using (var contentModeratorClient = new ContentModeratorClient(new ApiKeyServiceClientCredentials(_key)) { Endpoint = _endpoint })
            {
                try
                {
                    var language = "eng";
                    await contentModeratorClient.ListManagementTerm.AddTermAsync(termListId, term, language);
                    await contentModeratorClient.ListManagementTermLists.RefreshIndexMethodAsync(termListId, language);
                }
                catch(Exception e)
                {
                    throw new Exception($"The term: {term} could not be added to the term list with id: {termListId}");
                }
            }
        }

        public async Task<ContentModeratorTextResults> ScreenTextAsync(string text, string termListId)
        {
            if (termListId == string.Empty)
            {
                termListId = null;
            }

            var contentModeratorTextResults = new ContentModeratorTextResults();
                       
            try
            {
                using (var contentModeratorClient = new ContentModeratorClient(new ApiKeyServiceClientCredentials(_key)) { Endpoint = _endpoint })
                {
                    var byteArray = Encoding.UTF8.GetBytes(text);

                    using (var memoryStream = new MemoryStream(byteArray))
                    {
                        var screen = await contentModeratorClient.TextModeration.ScreenTextAsync("text/plain", memoryStream, listId: termListId);

                        AddTermsToContentModeratorTextResults(screen, contentModeratorTextResults);

                        AddCategoriesToContentModeratorTextResults(screen, contentModeratorTextResults);

                        AddPiiToContentModeratorTextResults(screen, contentModeratorTextResults);
                    }
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.Message, e);
            }

            return contentModeratorTextResults;
        }

        private void AddTermsToContentModeratorTextResults(Screen screen, ContentModeratorTextResults contentModeratorTextResults)
        {
            if (screen.Terms != null)
            {
                foreach (var term in screen.Terms)
                {
                    var termResultItem = new ContentModeratorTextResultItem
                    {
                        Text = term.Term,
                        Index = term.Index ?? 0,
                        Label = term.ListId.HasValue ? term.ListId.Value.ToString() : null
                    };

                    contentModeratorTextResults.ProfaneTerms.Add(termResultItem);
                }

                contentModeratorTextResults.DoesContainProfaneTerms = contentModeratorTextResults.ProfaneTerms.Count > 0;
            }            
        }

        private void AddCategoriesToContentModeratorTextResults(Screen screen, ContentModeratorTextResults contentModeratorTextResults)
        {
            if (screen.Classification != null)
            {
                contentModeratorTextResults.IsReviewRecommended = screen.Classification.ReviewRecommended ?? false;
                contentModeratorTextResults.SexuallyExplicitConfidence = screen.Classification.Category1.Score ?? 0.0;
                contentModeratorTextResults.SexuallySuggestiveConfidence = screen.Classification.Category2.Score ?? 0.0;
                contentModeratorTextResults.OffensiveConfidence = screen.Classification.Category3.Score ?? 0.0;
            }                      
        }

        private void AddPiiToContentModeratorTextResults(Screen screen, ContentModeratorTextResults contentModeratorTextResults)
        {
            if (screen.PII != null)
            {
                if (screen.PII.Address != null)
                {
                    foreach (var address in screen.PII.Address)
                    {
                        var resultItem = new ContentModeratorTextResultItem
                        {
                            Index = address.Index ?? 0,
                            Text = address.Text,
                            Label = "Address"
                        };
                        contentModeratorTextResults.Pii.Add(resultItem);
                    }
                }

                if (screen.PII.Email != null)
                {
                    foreach (var email in screen.PII.Email)
                    {
                        var resultItem = new ContentModeratorTextResultItem
                        {
                            Index = email.Index ?? 0,
                            Text = email.Text,
                            Label = "Email"
                        };
                        contentModeratorTextResults.Pii.Add(resultItem);
                    }
                }

                if (screen.PII.IPA != null)
                {
                    foreach (var ipa in screen.PII.IPA)
                    {
                        var resultItem = new ContentModeratorTextResultItem
                        {
                            Index = ipa.Index ?? 0,
                            Text = ipa.Text,
                            Label = "IPA"
                        };
                        contentModeratorTextResults.Pii.Add(resultItem);
                    }
                }

                if (screen.PII.Phone != null)
                {
                    foreach (var phone in screen.PII.Phone)
                    {
                        var resultItem = new ContentModeratorTextResultItem
                        {
                            Index = phone.Index ?? 0,
                            Text = phone.CountryCode + " " + phone.Text,
                            Label = "Phone"
                        };
                        contentModeratorTextResults.Pii.Add(resultItem);
                    }
                }

                if (screen.PII.SSN != null)
                {
                    foreach (var ssn in screen.PII.SSN)
                    {
                        var resultItem = new ContentModeratorTextResultItem
                        {
                            Index = ssn.Index ?? 0,
                            Text = ssn.Text,
                            Label = "SSN"
                        };
                        contentModeratorTextResults.Pii.Add(resultItem);
                    }
                }

                contentModeratorTextResults.DoesContainPii = contentModeratorTextResults.Pii.Count > 0;
            }            
        }
    }
}

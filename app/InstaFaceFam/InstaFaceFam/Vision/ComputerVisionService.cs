using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Xamarin.Forms.Internals;
using static Xamarin.Forms.Internals.GIFBitmap;

namespace InstaFaceFam.Vision
{
    public class ComputerVisionService : IComputerVisionService
    {
        private readonly ComputerVisionClient _computerVisionClient;

        public ComputerVisionService(string endpoint, string key)
        {
            _computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };
        }

        public async Task<ComputerVisionResults> GetComputerVisionResultsAsync(Stream stream)
        {
            var features = new List<VisualFeatureTypes>()
            {
              VisualFeatureTypes.Categories,
              VisualFeatureTypes.Description,
              VisualFeatureTypes.Faces,
              VisualFeatureTypes.ImageType,
              VisualFeatureTypes.Tags,
              VisualFeatureTypes.Adult,
              VisualFeatureTypes.Color,
              VisualFeatureTypes.Brands,
              VisualFeatureTypes.Objects
            };

            var computerVisionResults = new ComputerVisionResults();

            try
            {
                ImageAnalysis results = await _computerVisionClient.AnalyzeImageInStreamAsync(stream, features);

                AddMetaDataResultsToComputerVisionResults(results, computerVisionResults);

                AddColorResultsToComputerVisionResults(results, computerVisionResults);

                AddBrandResultsToComputerVisionResults(results, computerVisionResults);

                AddFaceResultsToComputerVisionResults(results, computerVisionResults);

                AddObjectResultsToComputerVisionResults(results, computerVisionResults);

                AddTagResultsToComputerVisionResults(results, computerVisionResults);

                AddDescriptionResultsToComputerVisionResults(results, computerVisionResults);

                AddAdultResultsToComputerVisionResults(results, computerVisionResults);

                AddCategoryResultsToComputerVisionResults(results, computerVisionResults);
            }
            catch(RequestFailedException e)
            {
                throw new Exception(e.Message, e);
            }

            return computerVisionResults;
        }

        private void AddMetaDataResultsToComputerVisionResults(ImageAnalysis results, ComputerVisionResults computerVisionResults)
        {
            computerVisionResults.ImageFormat = results.Metadata.Format;
            computerVisionResults.ImageWidth = results.Metadata.Width;
            computerVisionResults.ImageHeight = results.Metadata.Height;
        }

        private void AddColorResultsToComputerVisionResults(ImageAnalysis results, ComputerVisionResults computerVisionResults)
        {
            computerVisionResults.IsImageBlackAndWhite = results.Color.IsBWImg;
            computerVisionResults.ImageAccentColor = results.Color.AccentColor;
            computerVisionResults.ImageDominantBackgroundColor = results.Color.DominantColorBackground;
            computerVisionResults.ImageDominantForegroundColor = results.Color.DominantColorForeground;
            computerVisionResults.DominantColors.AddRange(results.Color.DominantColors);
        }

        private void AddBrandResultsToComputerVisionResults(ImageAnalysis results, ComputerVisionResults computerVisionResults)
        {
            if(results.Brands != null)
            {
                foreach(var brand in results.Brands)
                {
                    var brandResultItem = new ComputerVisionResultItem()
                    {
                        Name = brand.Name,
                        Confidence = brand.Confidence,
                        BoundingBox = new Rect(brand.Rectangle.X, brand.Rectangle.Y, brand.Rectangle.W, brand.Rectangle.H)
                    };
                    computerVisionResults.Brands.Add(brandResultItem);
                }
            }
        }

        private void AddFaceResultsToComputerVisionResults(ImageAnalysis results, ComputerVisionResults computerVisionResults)
        {
            if(results.Faces != null)
            {
                foreach(var face in results.Faces)
                {
                    var faceResultItem = new ComputerVisionResultItem()
                    {
                        Name = face.Gender.HasValue ? face.Gender.Value.ToString() : "",
                        Confidence = double.Parse(face.Age.ToString()),
                        BoundingBox = new Rect(face.FaceRectangle.Left, face.FaceRectangle.Top, face.FaceRectangle.Width,
                        face.FaceRectangle.Height)
                    };
                    computerVisionResults.Faces.Add(faceResultItem);
                }
            }
        }

        private void AddObjectResultsToComputerVisionResults(ImageAnalysis results, ComputerVisionResults computerVisionResults)
        {
            if(results.Objects != null)
            {
                foreach(var obj in results.Objects)
                {
                    var objectResultItem = new ComputerVisionResultItem()
                    {
                        Name = obj.ObjectProperty,
                        Confidence = obj.Confidence,
                        BoundingBox = new Rect(obj.Rectangle.X, obj.Rectangle.Y, obj.Rectangle.W, obj.Rectangle.H)
                    };
                    computerVisionResults.Objects.Add(objectResultItem);
                }
            }
        }

        private void AddTagResultsToComputerVisionResults(ImageAnalysis results, ComputerVisionResults computerVisionResults)
        {
            if(results.Tags != null)
            {
                foreach(var tag in results.Tags)
                {
                    var tagResultItem = new ComputerVisionResultItem
                    {
                        Name = tag.Name,
                        Confidence = tag.Confidence
                    };
                    computerVisionResults.Tags.Add(tagResultItem);
                }
            }
        }

        private void AddDescriptionResultsToComputerVisionResults(ImageAnalysis results, ComputerVisionResults computerVisionResults)
        {
            if(results.Description != null)
            {
                if(results.Description.Captions != null)
                {
                    foreach (var caption in results.Description.Captions)
                    {
                        var captionResultItem = new ComputerVisionResultItem()
                        {
                            Name = caption.Text,
                            Confidence = caption.Confidence
                        };
                        computerVisionResults.Captions.Add(captionResultItem);
                    }
                }

                if(results.Description.Tags != null)
                {
                    computerVisionResults.DescriptionTags.AddRange(results.Description.Tags);
                }
            }
        }

        private void AddAdultResultsToComputerVisionResults(ImageAnalysis results, ComputerVisionResults computerVisionResults)
        {
            computerVisionResults.IsAdultContent = results.Adult.IsAdultContent;
            computerVisionResults.AdultConfidence = results.Adult.AdultScore;
            computerVisionResults.IsRacyContent = results.Adult.IsRacyContent;
            computerVisionResults.RacyConfidence = results.Adult.RacyScore;
        }

        private void AddCategoryResultsToComputerVisionResults(ImageAnalysis results, ComputerVisionResults computerVisionResults)
        {
            if(results.Categories != null)
            {
                foreach(var category in results.Categories)
                {
                    var categoryResultItem = new ComputerVisionResultItem()
                    {
                        Name = category.Name,
                        Confidence = category.Score
                    };
                    computerVisionResults.Categories.Add(categoryResultItem);

                    if(category.Detail != null)
                    {
                        if(category.Detail.Celebrities != null)
                        {
                            foreach (var celebrity in category.Detail.Celebrities)
                            {
                                var celebrityResultItem = new ComputerVisionResultItem()
                                {
                                    Name = celebrity.Name,
                                    Confidence = celebrity.Confidence,
                                    BoundingBox = new Rect(celebrity.FaceRectangle.Left, celebrity.FaceRectangle.Top,
                                        celebrity.FaceRectangle.Width, celebrity.FaceRectangle.Height)
                                };
                                computerVisionResults.Celebrities.Add(celebrityResultItem);
                            }
                        }

                        if(category.Detail.Landmarks != null)
                        {
                            foreach(var landmark in category.Detail.Landmarks)
                            {
                                var landmarkResultItem = new ComputerVisionResultItem()
                                {
                                    Name = landmark.Name,
                                    Confidence = landmark.Confidence
                                };
                                computerVisionResults.Landmarks.Add(landmarkResultItem);
                            }
                        }
                    }
                }                
            }            
        }
    }
}

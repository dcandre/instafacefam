using System.Collections.Generic;

namespace InstaFaceFam.Vision
{
    public class ComputerVisionResults
    {
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public string ImageFormat { get; set; }
        public bool IsImageBlackAndWhite { get; set; }
        public string ImageAccentColor { get; set; }
        public string ImageDominantBackgroundColor { get; set; }
        public string ImageDominantForegroundColor { get; set; }
        public bool IsAdultContent { get; set; }
        public double AdultConfidence { get; set; }
        public bool IsRacyContent { get; set; }
        public double RacyConfidence { get; set; }
        public List<string> DescriptionTags { get; set; } = new List<string>();
        public List<string> DominantColors { get; set; } = new List<string>();
        public List<ComputerVisionResultItem> Categories { get; set; } = new List<ComputerVisionResultItem>();
        public List<ComputerVisionResultItem> Celebrities { get; set; } = new List<ComputerVisionResultItem>();
        public List<ComputerVisionResultItem> Landmarks { get; set; } = new List<ComputerVisionResultItem>();
        public List<ComputerVisionResultItem> Captions { get; set; } = new List<ComputerVisionResultItem>();        
        public List<ComputerVisionResultItem> Tags { get; set; } = new List<ComputerVisionResultItem>();
        public List<ComputerVisionResultItem> Objects { get; set; } = new List<ComputerVisionResultItem>();
        public List<ComputerVisionResultItem> Faces { get; set; } = new List<ComputerVisionResultItem>();
        public List<ComputerVisionResultItem> Brands { get; set; } = new List<ComputerVisionResultItem>();
    }
}

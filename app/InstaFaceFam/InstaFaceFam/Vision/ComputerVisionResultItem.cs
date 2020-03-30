using static Xamarin.Forms.Internals.GIFBitmap;

namespace InstaFaceFam.Vision
{
    public class ComputerVisionResultItem
    {
        public string Name { get; set; }
        public double Confidence { get; set; }
        public Rect BoundingBox { get; set; }
    }
}

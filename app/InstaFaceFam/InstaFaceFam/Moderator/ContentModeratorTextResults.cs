using System.Collections.Generic;

namespace InstaFaceFam.Moderator
{
    public class ContentModeratorTextResults
    {
        public bool IsReviewRecommended { get; set; }
        public double SexuallyExplicitConfidence { get; set; }
        public double SexuallySuggestiveConfidence { get; set; }
        public double OffensiveConfidence { get; set; }
        public bool DoesContainPii { get; set; }
        public bool DoesContainProfaneTerms { get; set; }
        public List<ContentModeratorTextResultItem> Pii { get; set; } = new List<ContentModeratorTextResultItem>();
        public List<ContentModeratorTextResultItem> ProfaneTerms { get; set; } = new List<ContentModeratorTextResultItem>();
    }
}

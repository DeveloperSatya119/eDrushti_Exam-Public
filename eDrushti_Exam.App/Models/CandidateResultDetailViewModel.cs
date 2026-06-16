namespace eDrushti_Exam.App.Models
{
    public class CandidateResultDetailViewModel
    {
        public int CandidateId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TrackName { get; set; } = string.Empty;
        public string TrackSlug { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public decimal? ScorePercent { get; set; }
        public string ResultStatus { get; set; } = "Pending";
        public string? PhotoPath { get; set; }
        public List<CandidateAnswer> Answers { get; set; } = new();
    }
}

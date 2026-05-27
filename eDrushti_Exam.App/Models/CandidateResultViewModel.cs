namespace eDrushti_Exam.App.Models
{
    public class CandidateResultViewModel
    {
        public int CandidateId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TrackName { get; set; } = string.Empty;
        public string TrackSlug { get; set; } = string.Empty;
        public int TotalAnswers { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}

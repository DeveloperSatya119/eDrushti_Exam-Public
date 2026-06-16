namespace eDrushti_Exam.App.Models
{
    public class Candidate
    {
        public int Id { get; set; }
        public int TrackId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsAdmin { get; set; } = false;
        public decimal? ScorePercent { get; set; }
        public string? ResultStatus { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool IsPhotoRequired { get; set; } = false;
        public bool PhotoConsentAccepted { get; set; } = false;
        public string? PhotoPath { get; set; }
        public DateTime? PhotoCapturedAt { get; set; }
        // Navigation
        public Track? Track { get; set; }
        public ICollection<CandidateAnswer> Answers { get; set; } = new List<CandidateAnswer>();
        public ICollection<CandidateQuestion> AssignedQuestions { get; set; } = new List<CandidateQuestion>();
        public ICollection<CandidateDraftAnswer> DraftAnswers { get; set; } = new List<CandidateDraftAnswer>();
    }
}

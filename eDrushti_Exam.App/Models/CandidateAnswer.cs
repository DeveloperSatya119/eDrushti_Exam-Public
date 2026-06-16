namespace eDrushti_Exam.App.Models
{
    public class CandidateAnswer
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        public int QuestionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public bool? IsCorrect { get; set; }
        public int Score { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Candidate? Candidate { get; set; }
        public Question? Question { get; set; }
    }
}

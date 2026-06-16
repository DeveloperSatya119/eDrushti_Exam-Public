namespace eDrushti_Exam.App.Models
{
    public class CandidateDraftAnswer
    {
        public int CandidateId { get; set; }
        public int QuestionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Candidate? Candidate { get; set; }
        public Question? Question { get; set; }
    }
}

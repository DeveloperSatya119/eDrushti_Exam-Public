namespace eDrushti_Exam.App.Models
{
    public class CandidateQuestion
    {
        public int CandidateId { get; set; }
        public int QuestionId { get; set; }
        public int OrderIndex { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public Candidate? Candidate { get; set; }
        public Question? Question { get; set; }
    }
}

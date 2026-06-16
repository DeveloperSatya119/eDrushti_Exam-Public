namespace eDrushti_Exam.App.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int TopicId { get; set; }               // FK → Topic (which belongs to a Track)
        public string QuestionText { get; set; } = string.Empty;
        public string? HintText { get; set; }                // optional hint for the candidate
        public string QuestionType { get; set; } = "Text";
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? CorrectAnswer { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Topic? Topic { get; set; }
        public ICollection<CandidateAnswer> CandidateAnswers { get; set; } = new List<CandidateAnswer>();
        public ICollection<CandidateQuestion> CandidateQuestions { get; set; } = new List<CandidateQuestion>();
    }
}

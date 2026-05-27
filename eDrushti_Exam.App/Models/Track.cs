namespace eDrushti_Exam.App.Models
{
    public class Track
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;  // e.g. "Java", ".NET", "QA"
        public string Slug { get; set; } = string.Empty;  // e.g. "java", "dotnet", "qa"
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Topic> Topics { get; set; } = new List<Topic>();
        public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
    }
}

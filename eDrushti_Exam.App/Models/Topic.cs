namespace eDrushti_Exam.App.Models
{
    public class Topic
    {
        public int Id { get; set; }
        public int TrackId { get; set; }
        public string Name { get; set; } = string.Empty;   // e.g. "Collections"
        public int SortOrder { get; set; }

        // Navigation
        public Track? Track { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}

namespace eDrushti_Exam.App.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalCandidates { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalSubmissions { get; set; }
        public int TotalTracks { get; set; }
        public List<Candidate> RecentCandidates { get; set; } = new();
    }
}

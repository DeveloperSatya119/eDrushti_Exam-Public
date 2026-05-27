using eDrushti_Exam.App.Models;

namespace eDrushti_Exam.App.Services
{
    public interface IAdminService
    {
        // ── Dashboard ─────────────────────────────────────────────────────────
        Task<AdminDashboardViewModel> GetDashboardStatsAsync();

        // ── Candidates ────────────────────────────────────────────────────────
        Task<List<Candidate>> GetAllCandidatesAsync();
        Task<Candidate?> GetCandidateByIdAsync(int id);
        Task<Candidate> CreateCandidateAsync(CandidateFormViewModel vm);
        Task<bool> UpdateCandidateAsync(CandidateFormViewModel vm);
        Task<bool> DeleteCandidateAsync(int id);
        Task<bool> ResetCandidateSubmissionAsync(int id);
        Task AssignQuestionsAsync(int candidateId, List<int> questionIds);

        // ── Questions ─────────────────────────────────────────────────────────
        Task<List<Question>> GetAllQuestionsAsync();
        Task<Question?> GetQuestionByIdAsync(int id);
        Task<Question> CreateQuestionAsync(QuestionFormViewModel vm);
        Task<bool> UpdateQuestionAsync(QuestionFormViewModel vm);
        Task<bool> DeleteQuestionAsync(int id);

        // ── Results ───────────────────────────────────────────────────────────
        Task<List<CandidateResultViewModel>> GetAllResultsAsync();
        Task<CandidateResultDetailViewModel?> GetCandidateResultAsync(int candidateId);

        // ── Tracks & Topics (for dropdowns) ───────────────────────────────────
        Task<List<Track>> GetActiveTracksAsync();
        Task<List<Topic>> GetTopicsByTrackAsync(int trackId);
        Task<List<Topic>> GetAllTopicsAsync();
    }
}

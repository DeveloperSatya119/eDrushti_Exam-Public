using eDrushti_Exam.App.Models;

namespace eDrushti_Exam.App.Services
{
    public interface IInterviewService
    {
        /// <summary>Returns all active topics + questions for the candidate's track.</summary>
        Task<List<Topic>> GetTopicsWithQuestionsAsync(int candidateId, int trackId);

        /// <summary>Saves all submitted answers for a candidate.</summary>
        Task SaveAnswersAsync(int candidateId, Dictionary<int, string> answers);

        Task<Dictionary<int, string>> GetDraftAnswersAsync(int candidateId);
        Task SaveDraftAnswersAsync(int candidateId, Dictionary<int, string> answers);

        /// <summary>Returns true if the candidate has already submitted.</summary>
        Task<bool> HasSubmittedAsync(int candidateId);

        Task<Candidate?> GetCandidatePhotoStateAsync(int candidateId);
        Task<bool> SaveCandidatePhotoAsync(int candidateId, string photoDataUrl, bool consentAccepted, string webRootPath);
    }
}

using eDrushti_Exam.App.Models;

namespace eDrushti_Exam.App.Services
{
    public interface IInterviewService
    {
        /// <summary>Returns all active topics + questions for the candidate's track.</summary>
        Task<List<Topic>> GetTopicsWithQuestionsAsync(int trackId);

        /// <summary>Saves all submitted answers for a candidate.</summary>
        Task SaveAnswersAsync(int candidateId, Dictionary<int, string> answers);

        /// <summary>Returns true if the candidate has already submitted.</summary>
        Task<bool> HasSubmittedAsync(int candidateId);
    }
}

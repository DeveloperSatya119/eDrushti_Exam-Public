using eDrushti_Exam.App.Models;

namespace eDrushti_Exam.App.Services
{
    public interface IAuthService
    {
        Task<Candidate?> ValidateCandidateAsync(string email, string password);
        Task<Candidate> RegisterCandidateAsync(string fullName, string email, string password, int trackId);
    }
}

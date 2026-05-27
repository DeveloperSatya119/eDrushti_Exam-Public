using eDrushti_Exam.App.Data;
using eDrushti_Exam.App.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace eDrushti_Exam.App.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly AppDbContext _db;

        public InterviewService(AppDbContext db) => _db = db;

        public async Task<List<Topic>> GetTopicsWithQuestionsAsync(int trackId)
        {
            return await _db.Topics.Where(t => t.TrackId == trackId).OrderBy(t => t.SortOrder).Include(t => t.Questions.Where(q => q.IsActive).OrderBy(q => q.OrderIndex)).ToListAsync();
        }

        public async Task SaveAnswersAsync(int candidateId, Dictionary<int, string> answers)
        {
            foreach (var (questionId, answerText) in answers)
            {
                if (string.IsNullOrWhiteSpace(answerText)) continue;

                _db.CandidateAnswers.Add(new CandidateAnswer
                {
                    CandidateId = candidateId,
                    QuestionId = questionId,
                    AnswerText = answerText.Trim(),
                    SubmittedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task<bool> HasSubmittedAsync(int candidateId)
        {
            return await _db.CandidateAnswers.AnyAsync(a => a.CandidateId == candidateId);
        }
    }
}

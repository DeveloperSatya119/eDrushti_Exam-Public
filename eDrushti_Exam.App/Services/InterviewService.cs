using eDrushti_Exam.App.Data;
using eDrushti_Exam.App.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace eDrushti_Exam.App.Services
{
    public class InterviewService : IInterviewService
    {
        private const decimal PassingPercent = 80m;

        private readonly AppDbContext _db;

        public InterviewService(AppDbContext db) => _db = db;

        public async Task<List<Topic>> GetTopicsWithQuestionsAsync(int candidateId, int trackId)
        {
            var assignedQuestionIds = await _db.CandidateQuestions
                .Where(cq => cq.CandidateId == candidateId)
                .OrderBy(cq => cq.OrderIndex)
                .Select(cq => cq.QuestionId)
                .ToListAsync();

            var topics = await _db.Topics
                .Where(t => t.TrackId == trackId)
                .OrderBy(t => t.SortOrder)
                .Include(t => t.Questions.Where(q => q.IsActive).OrderBy(q => q.OrderIndex))
                .ToListAsync();

            if (!assignedQuestionIds.Any())
                return topics;

            var assignedOrder = assignedQuestionIds
                .Select((id, index) => new { id, index })
                .ToDictionary(x => x.id, x => x.index);

            foreach (var topic in topics)
            {
                topic.Questions = topic.Questions
                    .Where(q => assignedOrder.ContainsKey(q.Id))
                    .OrderBy(q => assignedOrder[q.Id])
                    .ToList();
            }

            return topics.Where(t => t.Questions.Any()).ToList();
        }

        public async Task SaveAnswersAsync(int candidateId, Dictionary<int, string> answers)
        {
            var candidate = await _db.Candidates.FirstOrDefaultAsync(c => c.Id == candidateId);
            if (candidate == null) return;

            var assignedQuestionIds = await _db.CandidateQuestions
                .Where(cq => cq.CandidateId == candidateId)
                .Select(cq => cq.QuestionId)
                .ToListAsync();

            var allowedQuestionIds = assignedQuestionIds.Any()
                ? assignedQuestionIds
                : await _db.Questions
                    .Where(q => q.Topic != null && q.Topic.TrackId == candidate.TrackId && q.IsActive)
                    .Select(q => q.Id)
                    .ToListAsync();

            var questions = await _db.Questions
                .Where(q => allowedQuestionIds.Contains(q.Id))
                .ToDictionaryAsync(q => q.Id);

            var submittedAt = DateTime.UtcNow;

            foreach (var (questionId, answerText) in answers)
            {
                if (string.IsNullOrWhiteSpace(answerText) || !questions.TryGetValue(questionId, out var question))
                    continue;

                var normalizedAnswer = answerText.Trim();
                var isMcq = string.Equals(question.QuestionType, "MCQ", StringComparison.OrdinalIgnoreCase);
                bool? isCorrect = null;

                if (isMcq)
                {
                    isCorrect = string.Equals(
                        normalizedAnswer,
                        question.CorrectAnswer?.Trim(),
                        StringComparison.OrdinalIgnoreCase);
                }

                _db.CandidateAnswers.Add(new CandidateAnswer
                {
                    CandidateId = candidateId,
                    QuestionId = questionId,
                    AnswerText = normalizedAnswer,
                    IsCorrect = isCorrect,
                    Score = isCorrect == true ? 1 : 0,
                    SubmittedAt = submittedAt
                });
            }

            await _db.SaveChangesAsync();

            var totalScored = questions.Values.Count(q =>
                string.Equals(q.QuestionType, "MCQ", StringComparison.OrdinalIgnoreCase));

            var correct = await _db.CandidateAnswers
                .CountAsync(a => a.CandidateId == candidateId && a.IsCorrect == true);

            var scorePercent = totalScored == 0 ? 0m : Math.Round((decimal)correct * 100m / totalScored, 2);
            candidate.ScorePercent = scorePercent;
            candidate.ResultStatus = scorePercent >= PassingPercent ? "Pass" : "Fail";
            candidate.SubmittedAt = submittedAt;

            await _db.SaveChangesAsync();
        }

        public async Task<bool> HasSubmittedAsync(int candidateId)
        {
            return await _db.CandidateAnswers.AnyAsync(a => a.CandidateId == candidateId);
        }
    }
}

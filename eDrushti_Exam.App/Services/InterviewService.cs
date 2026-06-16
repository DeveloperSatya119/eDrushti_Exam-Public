using eDrushti_Exam.App.Data;
using eDrushti_Exam.App.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.RegularExpressions;

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

            var drafts = _db.CandidateDraftAnswers.Where(a => a.CandidateId == candidateId);
            _db.CandidateDraftAnswers.RemoveRange(drafts);

            await _db.SaveChangesAsync();
        }

        public async Task<Dictionary<int, string>> GetDraftAnswersAsync(int candidateId)
        {
            return await _db.CandidateDraftAnswers
                .Where(a => a.CandidateId == candidateId)
                .ToDictionaryAsync(a => a.QuestionId, a => a.AnswerText);
        }

        public async Task SaveDraftAnswersAsync(int candidateId, Dictionary<int, string> answers)
        {
            var allowedQuestionIds = await _db.CandidateQuestions
                .Where(cq => cq.CandidateId == candidateId)
                .Select(cq => cq.QuestionId)
                .ToListAsync();

            if (!allowedQuestionIds.Any())
            {
                var candidate = await _db.Candidates.FirstOrDefaultAsync(c => c.Id == candidateId);
                if (candidate == null) return;

                allowedQuestionIds = await _db.Questions
                    .Where(q => q.Topic != null && q.Topic.TrackId == candidate.TrackId && q.IsActive)
                    .Select(q => q.Id)
                    .ToListAsync();
            }

            var existing = await _db.CandidateDraftAnswers
                .Where(a => a.CandidateId == candidateId)
                .ToDictionaryAsync(a => a.QuestionId);

            var now = DateTime.UtcNow;
            foreach (var (questionId, answerText) in answers)
            {
                if (!allowedQuestionIds.Contains(questionId))
                    continue;

                var normalized = answerText?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(normalized))
                {
                    if (existing.TryGetValue(questionId, out var emptyDraft))
                        _db.CandidateDraftAnswers.Remove(emptyDraft);
                    continue;
                }

                if (existing.TryGetValue(questionId, out var draft))
                {
                    draft.AnswerText = normalized;
                    draft.UpdatedAt = now;
                }
                else
                {
                    _db.CandidateDraftAnswers.Add(new CandidateDraftAnswer
                    {
                        CandidateId = candidateId,
                        QuestionId = questionId,
                        AnswerText = normalized,
                        UpdatedAt = now
                    });
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task<bool> HasSubmittedAsync(int candidateId)
        {
            return await _db.CandidateAnswers.AnyAsync(a => a.CandidateId == candidateId);
        }

        public async Task<Candidate?> GetCandidatePhotoStateAsync(int candidateId)
        {
            return await _db.Candidates.FirstOrDefaultAsync(c => c.Id == candidateId);
        }

        public async Task<bool> SaveCandidatePhotoAsync(int candidateId, string photoDataUrl, bool consentAccepted, string webRootPath)
        {
            if (!consentAccepted || string.IsNullOrWhiteSpace(photoDataUrl))
                return false;

            var candidate = await _db.Candidates.FirstOrDefaultAsync(c => c.Id == candidateId);
            if (candidate == null || !candidate.IsPhotoRequired)
                return false;

            var match = Regex.Match(photoDataUrl, @"^data:image/(?<type>png|jpeg|jpg);base64,(?<data>.+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return false;

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(match.Groups["data"].Value);
            }
            catch
            {
                return false;
            }

            if (bytes.Length == 0 || bytes.Length > 5 * 1024 * 1024)
                return false;

            var photosRoot = Path.Combine(webRootPath, "candidate-photos");
            Directory.CreateDirectory(photosRoot);

            var fileName = $"{candidateId}-{DateTime.UtcNow:yyyyMMddHHmmss}.jpg";
            var absolutePath = Path.Combine(photosRoot, fileName);
            await File.WriteAllBytesAsync(absolutePath, bytes);

            candidate.PhotoConsentAccepted = true;
            candidate.PhotoPath = $"/candidate-photos/{fileName}";
            candidate.PhotoCapturedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return true;
        }
    }
}

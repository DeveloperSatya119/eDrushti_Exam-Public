using eDrushti_Exam.App.Data;
using eDrushti_Exam.App.Models;
using Microsoft.EntityFrameworkCore;

namespace eDrushti_Exam.App.Services
{
    public class AdminService : IAdminService
    {
        private const string AdminTrackSlug = "admin";
        private const int RandomQuestionsPerCandidate = 20;
        private const int MinimumDotNetQuestionsPerCandidate = 6;
        private const string DotNetTopicName = ".NET Core and C#";

        private readonly AppDbContext _db;

        public AdminService(AppDbContext db) => _db = db;

        private IQueryable<Candidate> VisibleCandidates()
            => _db.Candidates.Where(c => !c.IsAdmin);

        private IQueryable<Track> VisibleTracks()
            => _db.Tracks.Where(t => t.Slug != AdminTrackSlug);

        private IQueryable<Topic> VisibleTopics()
            => _db.Topics.Where(t => t.Track != null && t.Track.Slug != AdminTrackSlug);

        private IQueryable<Question> VisibleQuestions()
            => _db.Questions.Where(q => q.Topic != null && q.Topic.Track != null && q.Topic.Track.Slug != AdminTrackSlug);

        // ── Dashboard ─────────────────────────────────────────────────────────
        public async Task<AdminDashboardViewModel> GetDashboardStatsAsync()
        {
            return new AdminDashboardViewModel
            {
                TotalCandidates = await VisibleCandidates().CountAsync(),
                TotalQuestions = await VisibleQuestions().CountAsync(q => q.IsActive),
                TotalSubmissions = await _db.CandidateAnswers
                                        .Where(a => !a.Candidate!.IsAdmin)
                                        .Select(a => a.CandidateId).Distinct().CountAsync(),
                TotalTracks = await VisibleTracks().CountAsync(t => t.IsActive),
                RecentCandidates = await VisibleCandidates()
                                        .Include(c => c.Track)
                                        .Include(c => c.Answers)
                                        .OrderByDescending(c => c.CreatedAt)
                                        .Take(6)
                                        .ToListAsync()
            };
        }

        // ── Candidates ────────────────────────────────────────────────────────
        public async Task<List<Candidate>> GetAllCandidatesAsync()
        {
            return await VisibleCandidates()
                .Include(c => c.Track)
                .Include(c => c.Answers)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Candidate?> GetCandidateByIdAsync(int id)
        {
            return await VisibleCandidates()
                .Include(c => c.Track)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Candidate> CreateCandidateAsync(CandidateFormViewModel vm)
        {
            var candidate = new Candidate
            {
                FullName = vm.FullName.Trim(),
                Email = vm.Email.ToLower().Trim(),
                Phone = vm.Phone?.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password!),
                TrackId = vm.TrackId,
                IsActive = vm.IsActive,
                IsPhotoRequired = vm.IsPhotoRequired
            };
            _db.Candidates.Add(candidate);
            await _db.SaveChangesAsync();
            await AssignRandomQuestionsAsync(candidate.Id, candidate.TrackId);
            return candidate;
        }

        public async Task<bool> UpdateCandidateAsync(CandidateFormViewModel vm)
        {
            var c = await VisibleCandidates().FirstOrDefaultAsync(c => c.Id == vm.Id);
            if (c == null) return false;

            c.FullName = vm.FullName.Trim();
            c.Email = vm.Email.ToLower().Trim();
            c.Phone = vm.Phone?.Trim();
            c.TrackId = vm.TrackId;
            c.IsActive = vm.IsActive;
            c.IsPhotoRequired = vm.IsPhotoRequired;

            if (!c.IsPhotoRequired)
            {
                c.PhotoConsentAccepted = false;
                c.PhotoPath = null;
                c.PhotoCapturedAt = null;
            }

            if (!string.IsNullOrWhiteSpace(vm.Password))
                c.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password);

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCandidateAsync(int id)
        {
            var c = await VisibleCandidates().FirstOrDefaultAsync(c => c.Id == id);
            if (c == null) return false;
            _db.Candidates.Remove(c);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetCandidateSubmissionAsync(int id)
        {
            if (!await VisibleCandidates().AnyAsync(c => c.Id == id))
                return false;

            var answers = _db.CandidateAnswers.Where(a => a.CandidateId == id);
            _db.CandidateAnswers.RemoveRange(answers);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task AssignQuestionsAsync(int candidateId, List<int> questionIds)
        {
            if (!await VisibleCandidates().AnyAsync(c => c.Id == candidateId))
                return;

            var validQuestionIds = await VisibleQuestions()
                .Where(q => questionIds.Contains(q.Id))
                .Select(q => q.Id)
                .ToListAsync();

            var existing = _db.CandidateQuestions.Where(cq => cq.CandidateId == candidateId);
            _db.CandidateQuestions.RemoveRange(existing);

            var order = 1;
            foreach (var questionId in validQuestionIds)
            {
                _db.CandidateQuestions.Add(new CandidateQuestion
                {
                    CandidateId = candidateId,
                    QuestionId = questionId,
                    OrderIndex = order++
                });
            }

            await _db.SaveChangesAsync();
        }

        private async Task AssignRandomQuestionsAsync(int candidateId, int trackId)
        {
            var dotNetQuestionIds = await VisibleQuestions()
                .Where(q => q.IsActive
                    && q.Topic!.TrackId == trackId
                    && q.QuestionType == "MCQ"
                    && q.Topic.Name == DotNetTopicName)
                .OrderBy(q => Guid.NewGuid())
                .Take(MinimumDotNetQuestionsPerCandidate)
                .Select(q => q.Id)
                .ToListAsync();

            var remainingSlots = RandomQuestionsPerCandidate - dotNetQuestionIds.Count;
            var remainingQuestionIds = await VisibleQuestions()
                .Where(q => q.IsActive
                    && q.Topic!.TrackId == trackId
                    && q.QuestionType == "MCQ"
                    && !dotNetQuestionIds.Contains(q.Id))
                .OrderBy(q => Guid.NewGuid())
                .Take(remainingSlots)
                .Select(q => q.Id)
                .ToListAsync();

            var questionIds = dotNetQuestionIds.Concat(remainingQuestionIds)
                .OrderBy(_ => Guid.NewGuid())
                .ToList();

            var order = 1;
            foreach (var questionId in questionIds)
            {
                _db.CandidateQuestions.Add(new CandidateQuestion
                {
                    CandidateId = candidateId,
                    QuestionId = questionId,
                    OrderIndex = order++
                });
            }

            await _db.SaveChangesAsync();
        }

        // ── Questions ─────────────────────────────────────────────────────────
        public async Task<List<Question>> GetAllQuestionsAsync()
        {
            return await VisibleQuestions()
                .Include(q => q.Topic).ThenInclude(t => t!.Track)
                .OrderBy(q => q.Topic!.Track!.Name)
                .ThenBy(q => q.Topic!.SortOrder)
                .ThenBy(q => q.OrderIndex)
                .ToListAsync();
        }

        public async Task<Question?> GetQuestionByIdAsync(int id)
        {
            return await VisibleQuestions()
                .Include(q => q.Topic).ThenInclude(t => t!.Track)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Question> CreateQuestionAsync(QuestionFormViewModel vm)
        {
            var q = new Question
            {
                TopicId = vm.TopicId,
                QuestionText = vm.QuestionText.Trim(),
                HintText = vm.HintText?.Trim(),
                OrderIndex = vm.OrderIndex,
                IsActive = vm.IsActive
            };
            _db.Questions.Add(q);
            await _db.SaveChangesAsync();
            return q;
        }

        public async Task<bool> UpdateQuestionAsync(QuestionFormViewModel vm)
        {
            var q = await VisibleQuestions().FirstOrDefaultAsync(q => q.Id == vm.Id);
            if (q == null) return false;

            q.TopicId = vm.TopicId;
            q.QuestionText = vm.QuestionText.Trim();
            q.HintText = vm.HintText?.Trim();
            q.OrderIndex = vm.OrderIndex;
            q.IsActive = vm.IsActive;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuestionAsync(int id)
        {
            var q = await VisibleQuestions().FirstOrDefaultAsync(q => q.Id == id);
            if (q == null) return false;
            _db.Questions.Remove(q);
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Results ───────────────────────────────────────────────────────────
        public async Task<List<CandidateResultViewModel>> GetAllResultsAsync()
        {
            var candidates = await VisibleCandidates()
                .Include(c => c.Track)
                .Include(c => c.Answers)
                .Where(c => c.Answers.Any())
                .OrderByDescending(c => c.Answers.Max(a => a.SubmittedAt))
                .ToListAsync();

            return candidates.Select(c => new CandidateResultViewModel
            {
                CandidateId = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                TrackName = c.Track?.Name ?? "—",
                TrackSlug = c.Track?.Slug ?? "",
                TotalAnswers = c.Answers.Count,
                SubmittedAt = c.SubmittedAt ?? c.Answers.Max(a => a.SubmittedAt),
                ScorePercent = c.ScorePercent,
                ResultStatus = c.ResultStatus ?? "Pending"
            }).ToList();
        }

        public async Task<CandidateResultDetailViewModel?> GetCandidateResultAsync(int candidateId)
        {
            var candidate = await VisibleCandidates()
                .Include(c => c.Track)
                .FirstOrDefaultAsync(c => c.Id == candidateId);

            if (candidate == null) return null;

            var answers = await _db.CandidateAnswers
                .Where(a => a.CandidateId == candidateId)
                .Include(a => a.Question).ThenInclude(q => q!.Topic)
                .OrderBy(a => a.Question!.Topic!.SortOrder)
                .ThenBy(a => a.Question!.OrderIndex)
                .ToListAsync();

            return new CandidateResultDetailViewModel
            {
                CandidateId = candidate.Id,
                FullName = candidate.FullName,
                Email = candidate.Email,
                TrackName = candidate.Track?.Name ?? "—",
                TrackSlug = candidate.Track?.Slug ?? "",
                SubmittedAt = candidate.SubmittedAt ?? (answers.Any() ? answers.Max(a => a.SubmittedAt) : null),
                ScorePercent = candidate.ScorePercent,
                ResultStatus = candidate.ResultStatus ?? "Pending",
                PhotoPath = candidate.PhotoPath,
                Answers = answers
            };
        }

        // ── Dropdowns ─────────────────────────────────────────────────────────
        public async Task<List<Track>> GetActiveTracksAsync()
            => await VisibleTracks().Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();

        public async Task<List<Topic>> GetTopicsByTrackAsync(int trackId)
            => await VisibleTopics().Where(t => t.TrackId == trackId).OrderBy(t => t.SortOrder).ToListAsync();

        public async Task<List<Topic>> GetAllTopicsAsync()
            => await VisibleTopics().Include(t => t.Track).OrderBy(t => t.Track!.Name).ThenBy(t => t.SortOrder).ToListAsync();
    }
}

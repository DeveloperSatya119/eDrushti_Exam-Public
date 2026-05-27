using eDrushti_Exam.App.Data;
using eDrushti_Exam.App.Models;
using Microsoft.EntityFrameworkCore;

namespace eDrushti_Exam.App.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _db;

        public AdminService(AppDbContext db) => _db = db;

        // ── Dashboard ─────────────────────────────────────────────────────────
        public async Task<AdminDashboardViewModel> GetDashboardStatsAsync()
        {
            return new AdminDashboardViewModel
            {
                TotalCandidates = await _db.Candidates.CountAsync(),
                TotalQuestions = await _db.Questions.CountAsync(q => q.IsActive),
                TotalSubmissions = await _db.CandidateAnswers
                                        .Select(a => a.CandidateId).Distinct().CountAsync(),
                TotalTracks = await _db.Tracks.CountAsync(t => t.IsActive),
                RecentCandidates = await _db.Candidates
                .Where(c => c.Email != "hr@edrushti.in")
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
            return await _db.Candidates
                .Where(c => c.Email != "hr@edrushti.in")
                .Include(c => c.Track)
                .Include(c => c.Answers)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Candidate?> GetCandidateByIdAsync(int id)
        {
            return await _db.Candidates
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
                IsActive = vm.IsActive
            };
            _db.Candidates.Add(candidate);
            await _db.SaveChangesAsync();
            return candidate;
        }

        public async Task<bool> UpdateCandidateAsync(CandidateFormViewModel vm)
        {
            var c = await _db.Candidates.FindAsync(vm.Id);
            if (c == null) return false;

            c.FullName = vm.FullName.Trim();
            c.Email = vm.Email.ToLower().Trim();
            c.Phone = vm.Phone?.Trim();
            c.TrackId = vm.TrackId;
            c.IsActive = vm.IsActive;

            if (!string.IsNullOrWhiteSpace(vm.Password))
                c.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password);

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCandidateAsync(int id)
        {
            var c = await _db.Candidates.FindAsync(id);
            if (c == null) return false;
            _db.Candidates.Remove(c);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetCandidateSubmissionAsync(int id)
        {
            var answers = _db.CandidateAnswers.Where(a => a.CandidateId == id);
            _db.CandidateAnswers.RemoveRange(answers);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task AssignQuestionsAsync(int candidateId, List<int> questionIds)
        {
            // Store assignment in a separate CandidateQuestion join table
            // If you don't have that table yet, you can store it as JSON
            // or simply use this as a flag — for now we log the intent.
            // Replace with your actual assignment logic below:

            // Example if you have a CandidateQuestion table:
            // var existing = _db.CandidateQuestions.Where(cq => cq.CandidateId == candidateId);
            // _db.CandidateQuestions.RemoveRange(existing);
            // foreach (var qId in questionIds)
            //     _db.CandidateQuestions.Add(new CandidateQuestion { CandidateId = candidateId, QuestionId = qId });
            // await _db.SaveChangesAsync();

            // Placeholder — implement once CandidateQuestion table is added
            await Task.CompletedTask;
        }

        // ── Questions ─────────────────────────────────────────────────────────
        public async Task<List<Question>> GetAllQuestionsAsync()
        {
            return await _db.Questions
                .Include(q => q.Topic).ThenInclude(t => t!.Track)
                .OrderBy(q => q.Topic!.Track!.Name)
                .ThenBy(q => q.Topic!.SortOrder)
                .ThenBy(q => q.OrderIndex)
                .ToListAsync();
        }

        public async Task<Question?> GetQuestionByIdAsync(int id)
        {
            return await _db.Questions
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
            var q = await _db.Questions.FindAsync(vm.Id);
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
            var q = await _db.Questions.FindAsync(id);
            if (q == null) return false;
            _db.Questions.Remove(q);
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Results ───────────────────────────────────────────────────────────
        public async Task<List<CandidateResultViewModel>> GetAllResultsAsync()
        {
            var candidates = await _db.Candidates
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
                SubmittedAt = c.Answers.Max(a => a.SubmittedAt)
            }).ToList();
        }

        public async Task<CandidateResultDetailViewModel?> GetCandidateResultAsync(int candidateId)
        {
            var candidate = await _db.Candidates
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
                SubmittedAt = answers.Any() ? answers.Max(a => a.SubmittedAt) : null,
                Answers = answers
            };
        }

        // ── Dropdowns ─────────────────────────────────────────────────────────
        public async Task<List<Track>> GetActiveTracksAsync()
            => await _db.Tracks.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();

        public async Task<List<Topic>> GetTopicsByTrackAsync(int trackId)
            => await _db.Topics.Where(t => t.TrackId == trackId).OrderBy(t => t.SortOrder).ToListAsync();

        public async Task<List<Topic>> GetAllTopicsAsync()
            => await _db.Topics.Include(t => t.Track).OrderBy(t => t.Track!.Name).ThenBy(t => t.SortOrder).ToListAsync();
    }
}

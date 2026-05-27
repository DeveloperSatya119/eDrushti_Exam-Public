using eDrushti_Exam.App.Models;
using eDrushti_Exam.App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eDrushti_Exam.App.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IEmailService _emailService;

        public AdminController(IAdminService adminService, IEmailService emailService)
        {
            _adminService = adminService;
            _emailService = emailService;
        }

        [HttpGet("/Admin/Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var vm = await _adminService.GetDashboardStatsAsync();

            ViewBag.Tracks = await _adminService.GetActiveTracksAsync();
            ViewBag.Topics = await _adminService.GetAllTopicsAsync();
            ViewBag.AllCandidates = await _adminService.GetAllCandidatesAsync();
            ViewBag.AllQuestions = await _adminService.GetAllQuestionsAsync();
            ViewBag.Results = await _adminService.GetAllResultsAsync();

            return View("~/Views/Admin/Dashboard.cshtml", vm);
        }

        // GET /Admin/Questions/GetJson/{id}
        [HttpGet("/Admin/Questions/GetJson/{id}")]
        public async Task<IActionResult> GetQuestionJson(int id)
        {
            var q = await _adminService.GetQuestionByIdAsync(id);
            if (q == null) return NotFound();

            return Json(new
            {
                q.Id,
                q.TopicId,
                q.QuestionText,
                q.HintText,
                q.OrderIndex,
                q.IsActive,
                trackId = q.Topic?.Track?.Id
            });
        }

        // GET /Admin/Questions/TopicsByTrack/{trackId}
        [HttpGet("/Admin/Questions/TopicsByTrack/{trackId}")]
        public async Task<IActionResult> TopicsByTrack(int trackId)
        {
            var topics = await _adminService.GetTopicsByTrackAsync(trackId);
            return Json(topics.Select(t => new { t.Id, t.Name }));
        }

        // POST /Admin/Questions/Create
        [HttpPost("/Admin/Questions/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion([FromForm] QuestionFormViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.QuestionText))
                ModelState.AddModelError("QuestionText", "Question text is required.");

            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                });

            await _adminService.CreateQuestionAsync(vm);
            return Json(new { success = true });
        }

        // POST /Admin/Questions/Edit
        [HttpPost("/Admin/Questions/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion([FromForm] QuestionFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                });

            var ok = await _adminService.UpdateQuestionAsync(vm);
            return Json(new { success = ok });
        }

        // POST /Admin/Questions/Delete/{id}
        [HttpPost("/Admin/Questions/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var ok = await _adminService.DeleteQuestionAsync(id);
            return Json(new { success = ok, message = ok ? "Question deleted." : "Question not found." });
        }


        // GET /Admin/Candidates/GetJson/{id}
        [HttpGet("/Admin/Candidates/GetJson/{id}")]
        public async Task<IActionResult> GetCandidateJson(int id)
        {
            var c = await _adminService.GetCandidateByIdAsync(id);
            if (c == null) return NotFound();

            return Json(new
            {
                c.Id,
                c.FullName,
                c.Email,
                c.Phone,
                c.TrackId,
                c.IsActive
            });
        }

        // POST /Admin/Candidates/Create
        [HttpPost("/Admin/Candidates/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCandidate([FromForm] CandidateFormViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.AddModelError("Password", "Password is required for new candidates.");

            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                });

            await _adminService.CreateCandidateAsync(vm);
            return Json(new { success = true });
        }

        // POST /Admin/Candidates/Edit
        [HttpPost("/Admin/Candidates/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCandidate([FromForm] CandidateFormViewModel vm)
        {
            // Password is optional on edit — blank = keep existing hash
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.Remove("Password");

            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                });

            var ok = await _adminService.UpdateCandidateAsync(vm);
            return Json(new { success = ok });
        }

        // POST /Admin/Candidates/Delete/{id}
        [HttpPost("/Admin/Candidates/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCandidate(int id)
        {
            var ok = await _adminService.DeleteCandidateAsync(id);
            return Json(new { success = ok, message = ok ? "Candidate deleted." : "Candidate not found." });
        }

        // POST /Admin/Candidates/ResetSubmission/{id}
        [HttpPost("/Admin/Candidates/ResetSubmission/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetSubmission(int id)
        {
            await _adminService.ResetCandidateSubmissionAsync(id);
            return Json(new { success = true, message = "Submission reset successfully." });
        }

        // POST /Admin/Candidates/AssignQuestions
        [HttpPost("/Admin/Candidates/AssignQuestions")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignQuestions([FromForm] int candidateId,[FromForm] List<int> questionIds)
        {
            if (candidateId <= 0)
                return Json(new { success = false, errors = new[] { "Invalid candidate." } });

            if (questionIds == null || !questionIds.Any())
                return Json(new { success = false, errors = new[] { "Please select at least one question." } });

            await _adminService.AssignQuestionsAsync(candidateId, questionIds);
            return Json(new { success = true, message = $"{questionIds.Count} question(s) assigned." });
        }


        // GET /Admin/Results/{candidateId}/Json  — AJAX load for modal
        [HttpGet("/Admin/Results/{candidateId}/Json")]
        public async Task<IActionResult> ResultJson(int candidateId)
        {
            var detail = await _adminService.GetCandidateResultAsync(candidateId);
            if (detail == null) return NotFound();

            return Json(new
            {
                detail.CandidateId,
                detail.FullName,
                detail.Email,
                detail.TrackName,
                detail.TrackSlug,
                TotalAnswers = detail.Answers.Count,
                SubmittedAt = detail.SubmittedAt?.ToString("dd MMM yyyy, HH:mm"),
                Answers = detail.Answers
                    .OrderBy(a => a.Question?.Topic?.SortOrder)
                    .ThenBy(a => a.Question?.OrderIndex)
                    .Select(a => new
                    {
                        QuestionText = a.Question?.QuestionText ?? "—",
                        a.AnswerText,
                        TopicName = a.Question?.Topic?.Name ?? "—",
                        OrderIndex = a.Question?.OrderIndex ?? 0
                    })
            });
        }
        // POST /Admin/SendDecisionEmail
        [HttpPost("/Admin/SendDecisionEmail")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendDecisionEmail(int candidateId, string decision)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(decision))
                    return Json(new { success = false, error = "Decision is required." });

                var candidate = await _adminService.GetCandidateByIdAsync(candidateId);
                if (candidate == null)
                    return Json(new { success = false, error = "Candidate not found." });

                bool isPass = decision.Trim().ToLower() == "pass";
                string trackName = candidate.Track?.Name ?? "Technical";

                string subject = isPass
                    ? $"Congratulations — You Passed the {trackName} Interview"
                    : $"Interview Result — {trackName} Position";

                string body = isPass
                    ? $@"Dear {candidate.FullName},

                        We are pleased to inform you that you have successfully passed the {trackName} technical interview conducted on the eDrushti Exam Platform.

                        Our team was impressed with your responses. We will be in touch shortly regarding the next steps in the hiring process.

                        Congratulations and best of luck ahead!

                        Warm regards,
                        eDrushti Recruitment Team"
                                            : $@"Dear {candidate.FullName},

                        Thank you for taking the time to attempt the {trackName} technical interview on the eDrushti Exam Platform.

                        After a careful review of your responses, we regret to inform you that we will not be moving forward with your application at this time.

                        We appreciate your interest and encourage you to continue building your skills. You are welcome to apply again in the future.

                        Best regards,
                        eDrushti Recruitment Team";

                await _emailService.SendAsync(candidate.Email, subject, body);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
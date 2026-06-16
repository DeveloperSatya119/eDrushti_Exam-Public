using eDrushti_Exam.App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eDrushti_Exam.App.Controllers
{
    [Authorize]
    public class InterviewController : Controller
    {
        private readonly IInterviewService _interviewService;
        private readonly IWebHostEnvironment _environment;

        public InterviewController(IInterviewService interviewService, IWebHostEnvironment environment)
        {
            _interviewService = interviewService;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            int candidateId = GetCandidateId();
            int trackId = GetTrackId();

            if (await _interviewService.HasSubmittedAsync(candidateId))
                return View("AlreadySubmitted");

            var topics = await _interviewService.GetTopicsWithQuestionsAsync(candidateId, trackId);

            ViewBag.CandidateName = User.Identity?.Name;
            ViewBag.TrackName = User.FindFirst("TrackName")?.Value ?? "Interview";
            var candidate = await _interviewService.GetCandidatePhotoStateAsync(candidateId);
            ViewBag.IsPhotoRequired = candidate?.IsPhotoRequired ?? false;
            ViewBag.HasPhoto = !string.IsNullOrWhiteSpace(candidate?.PhotoPath);
            ViewBag.PhotoConsentAccepted = candidate?.PhotoConsentAccepted ?? false;
            ViewBag.DraftAnswers = await _interviewService.GetDraftAnswersAsync(candidateId);

            return View(topics);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePhoto([FromForm] string photoDataUrl, [FromForm] bool consentAccepted)
        {
            var candidateId = GetCandidateId();
            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var saved = await _interviewService.SaveCandidatePhotoAsync(candidateId, photoDataUrl, consentAccepted, webRoot);
            return Json(new { success = saved });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProgress(Dictionary<int, string> answers)
        {
            var candidateId = GetCandidateId();

            if (await _interviewService.HasSubmittedAsync(candidateId))
                return Json(new { success = false, message = "Test already submitted." });

            await _interviewService.SaveDraftAnswersAsync(candidateId, answers);
            return Json(new { success = true, savedAt = DateTime.Now.ToString("HH:mm") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Dictionary<int, string> answers)
        {
            int candidateId = GetCandidateId();

            if (await _interviewService.HasSubmittedAsync(candidateId))
                return View("AlreadySubmitted");

            await _interviewService.SaveAnswersAsync(candidateId, answers);

            TempData["Success"] = "Your answers have been submitted. Good luck!";
            return RedirectToAction("ThankYou");
        }

        public IActionResult ThankYou() => View();

        private int GetCandidateId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id) ? id : 0;

        private int GetTrackId() => int.TryParse(User.FindFirst("TrackId")?.Value, out int tid) ? tid : 0;
    }
}

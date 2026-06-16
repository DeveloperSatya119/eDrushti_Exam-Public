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

        public InterviewController(IInterviewService interviewService)
        {
            _interviewService = interviewService;
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

            return View(topics);
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

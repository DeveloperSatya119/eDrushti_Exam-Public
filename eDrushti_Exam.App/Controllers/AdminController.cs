using eDrushti_Exam.App.Models;
using eDrushti_Exam.App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace eDrushti_Exam.App.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IWebHostEnvironment _environment;

        public AdminController(IAdminService adminService, IWebHostEnvironment environment)
        {
            _adminService = adminService;
            _environment = environment;
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
                c.IsActive,
                c.IsPhotoRequired,
                hasPhoto = !string.IsNullOrWhiteSpace(c.PhotoPath)
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
                detail.TrackName,
                detail.TrackSlug,
                ScorePercent = detail.ScorePercent?.ToString("0.##") ?? "0",
                detail.ResultStatus,
                TotalAnswers = detail.Answers.Count,
                SubmittedAt = detail.SubmittedAt?.ToString("dd MMM yyyy, HH:mm"),
                Answers = detail.Answers
                    .OrderBy(a => a.Question?.Topic?.SortOrder)
                    .ThenBy(a => a.Question?.OrderIndex)
                    .Select(a => new
                    {
                        QuestionText = a.Question?.QuestionText ?? "—",
                        a.AnswerText,
                        QuestionType = a.Question?.QuestionType ?? "Text",
                        CorrectAnswer = a.Question?.CorrectAnswer ?? "",
                        a.IsCorrect,
                        TopicName = a.Question?.Topic?.Name ?? "—",
                        OrderIndex = a.Question?.OrderIndex ?? 0
                    })
            });
        }

        [HttpGet("/Admin/Results/{candidateId}/Pdf")]
        public async Task<IActionResult> ResultPdf(int candidateId)
        {
            var detail = await _adminService.GetCandidateResultAsync(candidateId);
            if (detail == null) return NotFound();

            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var pdf = BuildResultPdf(detail, webRoot);
            var safeName = string.Join("-", detail.FullName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Replace(" ", "-");
            return File(pdf, "application/pdf", $"answer-sheet-{safeName}-{candidateId}.pdf");
        }

        private static byte[] BuildResultPdf(CandidateResultDetailViewModel detail, string webRoot)
        {
            return StyledPdf(detail, webRoot);
        }

        private static byte[] StyledPdf(CandidateResultDetailViewModel detail, string webRoot)
        {
            var objects = new List<string>();
            var pages = new List<int>();
            var rows = detail.Answers
                .OrderBy(a => a.Question?.Topic?.SortOrder)
                .ThenBy(a => a.Question?.OrderIndex)
                .ToList();
            const int rowsPerPage = 6;
            var photo = TryLoadPdfPhoto(detail.PhotoPath, webRoot);

            objects.Add("<< /Type /Catalog /Pages 2 0 R >>");
            objects.Add("PAGES");
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");

            int? photoObjectNumber = null;
            if (photo != null)
            {
                objects.Add(BuildPhotoObject(photo.Value.Bytes, photo.Value.Width, photo.Value.Height));
                photoObjectNumber = objects.Count;
            }

            for (var pageStart = 0; pageStart < Math.Max(rows.Count, 1); pageStart += rowsPerPage)
            {
                var pageRows = rows.Skip(pageStart).Take(rowsPerPage).ToList();
                var content = new StringBuilder();

                Rect(content, 0, 792, 612, 50, "0.10 0.32 0.56");
                Text(content, "eDrushti Exam", 36, 810, 18, true, "1 1 1");
                Text(content, "Candidate Answer Sheet", 36, 794, 10, false, "0.85 0.93 1");

                var passed = string.Equals(detail.ResultStatus, "Pass", StringComparison.OrdinalIgnoreCase);
                Rect(content, 470, 798, 92, 26, passed ? "0.18 0.62 0.28" : "0.79 0.16 0.16");
                Text(content, $"{detail.ResultStatus} {detail.ScorePercent?.ToString("0.##") ?? "0"}%", 486, 807, 11, true, "1 1 1");

                Rect(content, 36, 708, 420, 64, "0.94 0.97 1");
                StrokeRect(content, 36, 708, 420, 64, "0.65 0.78 0.90");
                Text(content, "Candidate", 52, 750, 9, true, "0.28 0.34 0.40");
                Text(content, detail.FullName, 52, 732, 13, true, "0.08 0.10 0.13");
                Text(content, "Track", 240, 750, 9, true, "0.28 0.34 0.40");
                Text(content, detail.TrackName, 240, 732, 11, true, "0.08 0.10 0.13");
                Text(content, "Submitted", 52, 716, 9, true, "0.28 0.34 0.40");
                Text(content, detail.SubmittedAt?.ToString("dd MMM yyyy, HH:mm") ?? "-", 122, 716, 10, false, "0.08 0.10 0.13");

                if (photoObjectNumber.HasValue && pageStart == 0)
                {
                    StrokeRect(content, 470, 708, 92, 64, "0.65 0.78 0.90");
                    content.AppendLine("q");
                    content.AppendLine("92 0 0 64 470 708 cm");
                    content.AppendLine("/Photo Do");
                    content.AppendLine("Q");
                    Text(content, "Captured verification photo", 464, 696, 7, false, "0.28 0.34 0.40");
                }
                else if (pageStart == 0)
                {
                    StrokeRect(content, 470, 708, 92, 64, "0.82 0.86 0.90");
                    Text(content, "No photo", 496, 738, 10, true, "0.45 0.50 0.55");
                }

                var y = 672;
                if (!pageRows.Any())
                {
                    Text(content, "No submitted answers found.", 52, y, 12, false, "0.40 0.45 0.50");
                }

                for (var i = 0; i < pageRows.Count; i++)
                {
                    var answer = pageRows[i];
                    var qNo = pageStart + i + 1;
                    var top = y - (i * 98);

                    Rect(content, 36, top - 74, 540, 86, "1 1 1");
                    StrokeRect(content, 36, top - 74, 540, 86, "0.82 0.86 0.90");
                    Rect(content, 36, top - 8, 540, 20, "0.93 0.95 0.98");
                    Text(content, $"Q{qNo}. {answer.Question?.Topic?.Name ?? "General"}", 48, top - 1, 9, true, "0.10 0.32 0.56");

                    var questionText = answer.Question?.QuestionText ?? "-";
                    WriteWrapped(content, questionText, 48, top - 22, 92, 8, true, "0.08 0.10 0.13", 2);
                    Text(content, $"Answer: {answer.AnswerText}", 48, top - 50, 9, false, "0.20 0.24 0.28");

                    if (string.Equals(answer.Question?.QuestionType, "MCQ", StringComparison.OrdinalIgnoreCase))
                    {
                        var statusColor = answer.IsCorrect == true ? "0.18 0.62 0.28" : "0.79 0.16 0.16";
                        Text(content, $"Correct: {answer.Question?.CorrectAnswer ?? "-"}", 330, top - 50, 9, false, "0.20 0.24 0.28");
                        Text(content, answer.IsCorrect == true ? "Correct" : "Incorrect", 468, top - 50, 9, true, statusColor);
                    }
                }

                Text(content, $"Page {(pageStart / rowsPerPage) + 1}", 520, 28, 8, false, "0.45 0.50 0.55");

                var contentBytes = Encoding.ASCII.GetBytes(content.ToString());
                var contentObject = $"<< /Length {contentBytes.Length} >>\nstream\n{content}endstream";
                objects.Add(contentObject);
                var contentObjectNumber = objects.Count;

                var xObject = photoObjectNumber.HasValue ? $" /XObject << /Photo {photoObjectNumber.Value} 0 R >>" : "";
                objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 842] /Resources << /Font << /F1 3 0 R /F2 4 0 R >>{xObject} >> /Contents {contentObjectNumber} 0 R >>");
                pages.Add(objects.Count);
            }

            objects[1] = $"<< /Type /Pages /Kids [{string.Join(" ", pages.Select(p => $"{p} 0 R"))}] /Count {pages.Count} >>";

            var output = new StringBuilder();
            var offsets = new List<int> { 0 };
            output.AppendLine("%PDF-1.4");

            for (var index = 0; index < objects.Count; index++)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(output.ToString()));
                output.AppendLine($"{index + 1} 0 obj");
                output.AppendLine(objects[index]);
                output.AppendLine("endobj");
            }

            var xrefOffset = Encoding.ASCII.GetByteCount(output.ToString());
            output.AppendLine("xref");
            output.AppendLine($"0 {objects.Count + 1}");
            output.AppendLine("0000000000 65535 f ");
            for (var i = 1; i < offsets.Count; i++)
                output.AppendLine($"{offsets[i]:D10} 00000 n ");

            output.AppendLine("trailer");
            output.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
            output.AppendLine("startxref");
            output.AppendLine(xrefOffset.ToString());
            output.AppendLine("%%EOF");

            return Encoding.ASCII.GetBytes(output.ToString());
        }

        private static void Rect(StringBuilder content, int x, int y, int width, int height, string rgb)
        {
            content.AppendLine($"{rgb} rg");
            content.AppendLine($"{x} {y} {width} {height} re f");
        }

        private static void StrokeRect(StringBuilder content, int x, int y, int width, int height, string rgb)
        {
            content.AppendLine($"{rgb} RG");
            content.AppendLine($"{x} {y} {width} {height} re S");
        }

        private static void Text(StringBuilder content, string text, int x, int y, int size, bool bold, string rgb)
        {
            content.AppendLine("BT");
            content.AppendLine($"{rgb} rg");
            content.AppendLine($"/{(bold ? "F2" : "F1")} {size} Tf");
            content.AppendLine($"{x} {y} Td");
            content.AppendLine($"({EscapePdf(ToPdfAscii(text))}) Tj");
            content.AppendLine("ET");
        }

        private static void WriteWrapped(StringBuilder content, string text, int x, int y, int width, int size, bool bold, string rgb, int maxLines)
        {
            var lines = Wrap(ToPdfAscii(text), width).Take(maxLines).ToList();
            for (var i = 0; i < lines.Count; i++)
                Text(content, lines[i], x, y - (i * (size + 4)), size, bold, rgb);
        }

        private static IEnumerable<string> Wrap(string value, int width)
        {
            value = value.Replace("\r", " ").Replace("\n", " ");
            for (var i = 0; i < value.Length; i += width)
                yield return value.Substring(i, Math.Min(width, value.Length - i));
        }

        private static string EscapePdf(string value)
            => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

        private static string ToPdfAscii(string value)
            => Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));

        private static (byte[] Bytes, int Width, int Height)? TryLoadPdfPhoto(string? photoPath, string webRoot)
        {
            if (string.IsNullOrWhiteSpace(photoPath))
                return null;

            var relative = photoPath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            var path = Path.Combine(webRoot, relative);
            if (!System.IO.File.Exists(path))
                return null;

            var bytes = System.IO.File.ReadAllBytes(path);
            var size = TryGetJpegSize(bytes);
            return size == null ? null : (bytes, size.Value.Width, size.Value.Height);
        }

        private static (int Width, int Height)? TryGetJpegSize(byte[] bytes)
        {
            for (var i = 2; i < bytes.Length - 9; i++)
            {
                if (bytes[i] != 0xFF)
                    continue;

                var marker = bytes[i + 1];
                if (marker is 0xC0 or 0xC1 or 0xC2 or 0xC3)
                {
                    var height = (bytes[i + 5] << 8) + bytes[i + 6];
                    var width = (bytes[i + 7] << 8) + bytes[i + 8];
                    return (width, height);
                }
            }

            return null;
        }

        private static string BuildPhotoObject(byte[] bytes, int width, int height)
        {
            var hex = Convert.ToHexString(bytes) + ">";
            return $"<< /Type /XObject /Subtype /Image /Width {width} /Height {height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter [/ASCIIHexDecode /DCTDecode] /Length {hex.Length} >>\nstream\n{hex}\nendstream";
        }
    }
}

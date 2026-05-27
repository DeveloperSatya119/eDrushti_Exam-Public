using System.ComponentModel.DataAnnotations;

namespace eDrushti_Exam.App.Models
{
    public class QuestionFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a topic.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid topic.")]
        public int TopicId { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        [MinLength(10, ErrorMessage = "Question must be at least 10 characters.")]
        [MaxLength(2000, ErrorMessage = "Question cannot exceed 2000 characters.")]
        public string QuestionText { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Hint cannot exceed 500 characters.")]
        public string? HintText { get; set; }

        [Range(1, 999, ErrorMessage = "Order must be between 1 and 999.")]
        public int OrderIndex { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        // Read-only — populated when loading edit form, not posted back
        public string? TopicName { get; set; }
        public string? TrackName { get; set; }
        public int? TrackId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}

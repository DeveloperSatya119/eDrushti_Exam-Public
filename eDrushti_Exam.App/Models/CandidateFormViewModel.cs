using System.ComponentModel.DataAnnotations;

namespace eDrushti_Exam.App.Models
{
    public class CandidateFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        // Required on Create (Id == 0), optional on Edit (blank = keep existing)
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Please select a track.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid track.")]
        public int TrackId { get; set; }

        public bool IsActive { get; set; } = true;

        // Read-only — populated when loading edit form, not posted back
        public string? TrackName { get; set; }
        public bool HasSubmitted { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}

using eDrushti_Exam.App.Data;
using eDrushti_Exam.App.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace eDrushti_Exam.App.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;

        public AuthService(AppDbContext db) => _db = db;

        public async Task<Candidate?> ValidateCandidateAsync(string email, string password)
        {
            var candidate = await _db.Candidates.Include(c => c.Track).FirstOrDefaultAsync(c => c.Email == email.ToLower().Trim() && c.IsActive);

            if (candidate == null) return null;

            return BCrypt.Net.BCrypt.Verify(password, candidate.PasswordHash) ? candidate : null;
        }

        public async Task<Candidate> RegisterCandidateAsync(
            string fullName, string email, string password, int trackId)
        {
            var candidate = new Candidate
            {
                FullName = fullName,
                Email = email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                TrackId = trackId
            };

            _db.Candidates.Add(candidate);
            await _db.SaveChangesAsync();
            return candidate;
        }
    }
}

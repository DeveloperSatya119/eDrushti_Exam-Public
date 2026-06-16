using eDrushti_Exam.App.Models;
using Microsoft.EntityFrameworkCore;

namespace eDrushti_Exam.App.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Track> Tracks { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<CandidateAnswer> CandidateAnswers { get; set; }
        public DbSet<CandidateQuestion> CandidateQuestions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Track>(e =>
            {
                e.HasIndex(t => t.Slug).IsUnique();
                e.Property(t => t.Name).HasMaxLength(100).IsRequired();
                e.Property(t => t.Slug).HasMaxLength(60).IsRequired();
                e.Property(t => t.Description).HasMaxLength(500);
            });


            modelBuilder.Entity<Topic>(e =>
            {
                e.Property(t => t.Name).HasMaxLength(150).IsRequired();
                e.HasOne(t => t.Track)
                 .WithMany(tr => tr.Topics)
                 .HasForeignKey(t => t.TrackId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Question>(e =>
            {
                e.Property(q => q.QuestionText).HasMaxLength(2000).IsRequired();
                e.Property(q => q.HintText).HasMaxLength(500);
                e.Property(q => q.QuestionType).HasMaxLength(20).IsRequired();
                e.Property(q => q.OptionA).HasMaxLength(500);
                e.Property(q => q.OptionB).HasMaxLength(500);
                e.Property(q => q.OptionC).HasMaxLength(500);
                e.Property(q => q.OptionD).HasMaxLength(500);
                e.Property(q => q.CorrectAnswer).HasMaxLength(10);
                e.HasOne(q => q.Topic)
                 .WithMany(t => t.Questions)
                 .HasForeignKey(q => q.TopicId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Candidate>(e =>
            {
                e.HasIndex(c => c.Email).IsUnique();
                e.Property(c => c.FullName).HasMaxLength(150).IsRequired();
                e.Property(c => c.Email).HasMaxLength(200).IsRequired();
                e.Property(c => c.Phone).HasMaxLength(20);
                e.Property(c => c.ScorePercent).HasColumnType("decimal(5,2)");
                e.Property(c => c.ResultStatus).HasMaxLength(20);
                e.HasOne(c => c.Track)
                 .WithMany(t => t.Candidates)
                 .HasForeignKey(c => c.TrackId)
                 .OnDelete(DeleteBehavior.Restrict); 
            });


            modelBuilder.Entity<CandidateAnswer>(e =>
            {
                e.HasIndex(a => new { a.CandidateId, a.QuestionId }).IsUnique();
                e.HasOne(a => a.Candidate)
                 .WithMany(c => c.Answers)
                 .HasForeignKey(a => a.CandidateId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(a => a.Question)
                 .WithMany(q => q.CandidateAnswers)
                 .HasForeignKey(a => a.QuestionId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CandidateQuestion>(e =>
            {
                e.HasKey(cq => new { cq.CandidateId, cq.QuestionId });
                e.HasIndex(cq => new { cq.CandidateId, cq.OrderIndex }).IsUnique();
                e.HasOne(cq => cq.Candidate)
                 .WithMany(c => c.AssignedQuestions)
                 .HasForeignKey(cq => cq.CandidateId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(cq => cq.Question)
                 .WithMany(q => q.CandidateQuestions)
                 .HasForeignKey(cq => cq.QuestionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

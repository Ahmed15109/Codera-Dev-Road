using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;

namespace progect_DEPI.Models
{
	public class ApplicationDbContext: IdentityDbContext
	{
        public ApplicationDbContext()
        {
            
        }
        public DbSet<User> Users { get; set; }
       
        public DbSet<Course> Courses { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Lesson> Lessons { get; set; }
		public DbSet<Enrollment> Enrollments { get; set; }
		public DbSet<Payment> Payments { get; set; }
		public DbSet<Review> Reviews { get; set; }
		public DbSet<Quizzes> Quizzes { get; set; }
		public DbSet<Certificate> Certificates { get; set; }
		public DbSet<Notification> Notifications { get; set; }
        public DbSet<QuizResult> QuizResults { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
	}
}

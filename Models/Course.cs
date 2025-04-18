using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace progect_DEPI.Models
{
	public class Course
	{
		public int CourseId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdateAt { get; set; }

		public virtual ICollection<Lesson> Lessons { get; set; }
		public virtual ICollection<Enrollment> Enrollments { get; set; }
		public virtual ICollection<Review> Reviews { get; set; }
		public virtual ICollection<Quiz> Quizzes { get; set; }

		public int CategoryId { get; set; }
		public virtual Category Category { get; set; }
	}
}

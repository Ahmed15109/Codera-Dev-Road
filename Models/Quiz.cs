namespace progect_DEPI.Models
{
	public class Quiz
	{
		public int QuizId { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public TimeSpan Time { get; set; }
		public string Status { get; set; }
		public DateTime CreatedAt { get; set; }

		public int UserId { get; set; }
		public virtual User User { get; set; }

		public int CourseId { get; set; }
		public virtual Course Course { get; set; }
	}
}

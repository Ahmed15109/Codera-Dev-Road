namespace progect_DEPI.Models
{
	public class Review
	{
		public int ReviewId { get; set; }
		public int Rating { get; set; }
		public string Comment { get; set; }
		public DateTime CreatedAt { get; set; }

		public int UserId { get; set; }
		public virtual User User { get; set; }

		public int CourseId { get; set; }
		public virtual Course Course { get; set; }
	}
}

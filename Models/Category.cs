namespace progect_DEPI.Models
{
	public class Category
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; }
		public string Description { get; set; }
		public int LessonsCount { get; set; }
		public DateTime? UpdateAt { get; set; }

		public virtual ICollection<Course> Courses { get; set; }
	}
}

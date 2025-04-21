namespace progect_DEPI.Models
{
	public class Lesson
	{
		public int LessonId { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public int OrderNumber { get; set; }
		public string Level { get; set; }
        public string VideoUrl { get; set; }
        public string ImageUrl { get; set; }
        public int CourseId { get; set; }
		public virtual Course Course { get; set; }
	}
}
